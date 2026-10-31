using System.Security.Claims;
using System.Text.Json;
using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Data;
using MeetCampus.Data.Audit;
using MeetCampus.Data.Questionnaire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Features.Administration.Services;

public sealed class QuestionnaireAdministrationService(
    ApplicationDbContext dbContext,
    IAuthorizationService authorizationService)
{
    public async Task<Question> CreateQuestionAsync(
        ClaimsPrincipal actor,
        Question question,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(question);
        var actorId = await RequireAdministratorAsync(actor);
        ValidateQuestion(question);
        question.Id = question.Id == Guid.Empty ? Guid.NewGuid() : question.Id;
        question.UpdatedAt = DateTimeOffset.UtcNow;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.Questions.Add(question);
        dbContext.AuditEvents.Add(CreateAuditEvent(actorId, "Question.Created", question.Id.ToString(), new { question.Code }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return question;
    }

    public async Task<List<Question>> GetQuestionsAsync(
        ClaimsPrincipal actor,
        CancellationToken cancellationToken = default)
    {
        await RequireAdministratorAsync(actor);
        return await dbContext.Questions
            .AsNoTracking()
            .Include(question => question.Translations)
            .Include(question => question.Options)
                .ThenInclude(option => option.Translations)
            .OrderBy(question => question.SortOrder)
            .ThenBy(question => question.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<QuestionCatalogResponse> GetQuestionnaireAdminCatalogAsync(ClaimsPrincipal actor)
    {
        await RequireAdministratorAsync(actor);
        return QuestionnaireAdminCatalog.Value;
    }

    public async Task UpdateQuestionAsync(
        ClaimsPrincipal actor,
        Guid questionId,
        Question updatedQuestion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updatedQuestion);
        var actorId = await RequireAdministratorAsync(actor);
        ValidateQuestion(updatedQuestion);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var question = await dbContext.Questions
            .Include(item => item.Options)
                .ThenInclude(option => option.Translations)
            .Include(item => item.Translations)
            .SingleOrDefaultAsync(item => item.Id == questionId, cancellationToken)
            ?? throw new KeyNotFoundException("Question was not found.");

        if (!string.Equals(question.Code, updatedQuestion.Code, StringComparison.Ordinal))
            throw new InvalidOperationException("A question's stable ID cannot be changed because stored answers refer to it.");

        question.Code = updatedQuestion.Code;
        SynchronizeTranslations(question.Translations, updatedQuestion.Translations, translation => translation.CultureCode, translation => translation.Text,
            translation => translation.CultureCode,
            (translation, text) => translation.Text = text,
            (cultureCode, promptText) => new QuestionTranslation { QuestionId = question.Id, CultureCode = cultureCode, Text = promptText });
        question.IsRequired = updatedQuestion.IsRequired;
        question.Type = updatedQuestion.Type;
        question.Group = updatedQuestion.Group;
        question.ScoringWeight = updatedQuestion.ScoringWeight;
        question.SortOrder = updatedQuestion.SortOrder;
        question.MaxSelections = updatedQuestion.MaxSelections;
        question.ScaleMin = updatedQuestion.ScaleMin;
        question.ScaleMax = updatedQuestion.ScaleMax;

        var updatedCodes = updatedQuestion.Options.Select(option => option.OptionCode).ToHashSet(StringComparer.Ordinal);
        var removedOptions = question.Options.Where(option => !updatedCodes.Contains(option.OptionCode)).ToList();
        if (removedOptions.Count > 0)
        {
            var removedOptionIds = removedOptions.Select(option => option.Id).ToArray();
            if (await dbContext.UserAnswers.AnyAsync(answer => removedOptionIds.Contains(answer.QuestionOptionId), cancellationToken))
                throw new InvalidOperationException("Options referenced by saved answers cannot be removed.");
            foreach (var removedOption in removedOptions)
                question.Options.Remove(removedOption);
            dbContext.QuestionOptions.RemoveRange(removedOptions);
        }

        foreach (var updatedOption in updatedQuestion.Options)
        {
            var option = question.Options.SingleOrDefault(item => item.OptionCode == updatedOption.OptionCode);
            if (option is null)
            {
                option = new QuestionOption { Id = Guid.NewGuid(), QuestionId = question.Id, OptionCode = updatedOption.OptionCode };
                dbContext.QuestionOptions.Add(option);
                question.Options.Add(option);
            }

            option.SortOrder = updatedOption.SortOrder;
            option.ScoringValue = updatedOption.ScoringValue;
            SynchronizeTranslations(option.Translations, updatedOption.Translations, translation => translation.CultureCode, translation => translation.Text,
                translation => translation.CultureCode,
                (translation, text) => translation.Text = text,
                (cultureCode, labelText) => new QuestionOptionTranslation { QuestionOptionId = option.Id, CultureCode = cultureCode, Text = labelText });
        }
        question.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.AuditEvents.Add(CreateAuditEvent(actorId, "Question.Updated", question.Id.ToString(), new { question.Code, question.UpdatedAt }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteQuestionAsync(
        ClaimsPrincipal actor,
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        var actorId = await RequireAdministratorAsync(actor);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var question = await dbContext.Questions
            .Include(item => item.Options)
            .SingleOrDefaultAsync(item => item.Id == questionId, cancellationToken)
            ?? throw new KeyNotFoundException("Question was not found.");

        var optionIds = question.Options.Select(option => option.Id).ToArray();
        if (optionIds.Length > 0 && await dbContext.UserAnswers
                .AnyAsync(answer => optionIds.Contains(answer.QuestionOptionId), cancellationToken))
            throw new InvalidOperationException("Questions referenced by saved answers cannot be deleted.");

        dbContext.AuditEvents.Add(CreateAuditEvent(actorId, "Question.Deleted", question.Id.ToString(), new { question.Code }));
        dbContext.Questions.Remove(question);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static void ValidateQuestion(Question question)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question.Code);
        ArgumentException.ThrowIfNullOrWhiteSpace(question.Group);

        if (!Enum.IsDefined(question.Type))
            throw new ArgumentException("Question type must be supported.", nameof(question));

        if (question.ScoringWeight is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(question), "Question scoring weight must be between zero and one.");

        if (!QuestionnaireAdminCatalog.Value.Categories.Any(category => category.Code == question.Group))
            throw new ArgumentException("Question category is not supported.", nameof(question));

        if (question.Translations.Count != 2 || !HasTranslations(question.Translations.Select(item => (item.CultureCode, item.Text))))
            throw new ArgumentException("Question prompts are required in English and French.", nameof(question));

        if ((question.Type == QuestionType.MultipleChoice || question.Type == QuestionType.SingleChoice) && question.Options.Count == 0)
            throw new ArgumentException("Choice questions must define at least one option.", nameof(question));

        if (question.Type == QuestionType.Scale && (question.Options.Count != 0 || question.ScaleMin is null || question.ScaleMax is null || question.ScaleMin >= question.ScaleMax || question.MaxSelections is not null))
            throw new ArgumentException("Scale questions require a valid range and cannot define choices or a selection limit.", nameof(question));

        if (question.Type != QuestionType.Scale && (question.ScaleMin is not null || question.ScaleMax is not null))
            throw new ArgumentException("Only scale questions may define scale bounds.", nameof(question));

        if (question.Type != QuestionType.MultipleChoice && question.MaxSelections is not null)
            throw new ArgumentException("Only multiple-choice questions may define a selection limit.", nameof(question));

        if (question.MaxSelections is <= 0 || question.MaxSelections > question.Options.Count)
            throw new ArgumentException("The selection limit must be positive and no greater than the option count.", nameof(question));

        if (question.Options.Any(option => option.SortOrder < 0))
            throw new ArgumentException("Option sort order cannot be negative.", nameof(question));

        if (question.Options.Any(option => string.IsNullOrWhiteSpace(option.OptionCode)
                || option.Translations.Count != 2
                || !HasTranslations(option.Translations.Select(item => (item.CultureCode, item.Text)))))
            throw new ArgumentException("Question options require stable codes and English and French labels.", nameof(question));

        if (question.Options.Select(option => option.OptionCode).Distinct(StringComparer.Ordinal).Count() != question.Options.Count
            || question.Options.Select(option => option.SortOrder).Distinct().Count() != question.Options.Count)
            throw new ArgumentException("Question option IDs and sort orders must be unique.", nameof(question));
    }

    private static bool HasTranslations(IEnumerable<(string CultureCode, string Text)> translations)
    {
        var items = translations.ToList();
        return items.Count == 2
            && items.All(item => !string.IsNullOrWhiteSpace(item.Text))
            && items.Select(item => item.CultureCode).ToHashSet(StringComparer.Ordinal).SetEquals(["en", "fr"]);
    }

    private static void SynchronizeTranslations<TTranslation>(
        ICollection<TTranslation> current,
        IEnumerable<TTranslation> updated,
        Func<TTranslation, string> cultureSelector,
        Func<TTranslation, string> textSelector,
        Func<TTranslation, string> updatedCultureSelector,
        Action<TTranslation, string> setText,
        Func<string, string, TTranslation> create)
    {
        foreach (var translation in updated)
        {
            var cultureCode = updatedCultureSelector(translation);
            var existing = current.SingleOrDefault(item => cultureSelector(item) == cultureCode);
            if (existing is null)
                current.Add(create(cultureCode, textSelector(translation)));
            else
                setText(existing, textSelector(translation));
        }
    }

    private async Task<Guid> RequireAdministratorAsync(ClaimsPrincipal actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        var authorizationResult = await authorizationService.AuthorizeAsync(actor, "AdministratorOnly");
        if (!authorizationResult.Succeeded)
            throw new UnauthorizedAccessException("Administrator authorization is required.");

        var actorId = actor.FindFirstValue(ClaimTypes.NameIdentifier);
        return !Guid.TryParse(actorId, out var parsedActorId)
            ? throw new UnauthorizedAccessException("An authenticated administrator identity is required.")
            : parsedActorId;
    }

    private static AuditEvent CreateAuditEvent(Guid actorId, string action, string entityId, object details) => new()
    {
        Id = Guid.NewGuid(),
        ActorUserId = actorId,
        Action = action,
        EntityType = "QuestionDefinition",
        EntityId = entityId,
        OccurredAt = DateTimeOffset.UtcNow,
        DetailsJson = JsonSerializer.Serialize(details)
    };
}
