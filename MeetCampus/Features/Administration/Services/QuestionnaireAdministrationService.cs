using System.Text.Json;
using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Data;
using MeetCampus.Data.Audit;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.User;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Features.Administration.Services;

public sealed class QuestionnaireAdministrationService(
    ApplicationDbContext dbContext,
    IValidator<Question> questionValidator)
{
    public async Task<Question> CreateQuestionAsync(
        ApplicationUser actor,
        Question question,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(question);
        ArgumentNullException.ThrowIfNull(actor);
        var result = await questionValidator.ValidateAsync(question, cancellationToken);
        if (!result.IsValid)
            throw new ArgumentException(string.Join(" ", result.Errors.Select(error => error.ErrorMessage)), nameof(question));

        question.Id = question.Id == Guid.Empty ? Guid.NewGuid() : question.Id;
        question.UpdatedAt = DateTimeOffset.UtcNow;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.Questions.Add(question);
        dbContext.AuditEvents.Add(CreateAuditEvent(actor.Id, "Question.Created", question.Id.ToString(), new { question.Code }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return question;
    }

    public async Task<List<Question>> GetQuestionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Questions
            .AsNoTracking()
            .Include(question => question.Category)
            .Include(question => question.Translations)
            .Include(question => question.Options)
                .ThenInclude(option => option.Translations)
            .OrderBy(question => question.SortOrder)
            .ThenBy(question => question.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<QuestionCatalogResponse> GetQuestionnaireAdminCatalogAsync(CancellationToken cancellationToken = default)
    {
        var categories = await dbContext.QuestionCategories
            .AsNoTracking()
            .OrderBy(category => category.SortOrder)
            .Select(category => new QuestionCategoryOption(category.Id, category.NameEn, category.NameFr))
            .ToListAsync(cancellationToken);
        return new QuestionCatalogResponse(categories);
    }

    public async Task UpdateQuestionAsync(
        ApplicationUser actor,
        Guid questionId,
        Question updatedQuestion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updatedQuestion);
        ArgumentNullException.ThrowIfNull(actor);
        var result = await questionValidator.ValidateAsync(updatedQuestion, cancellationToken);
        if (!result.IsValid)
            throw new ArgumentException(string.Join(" ", result.Errors.Select(error => error.ErrorMessage)), nameof(updatedQuestion));
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
        question.Type = updatedQuestion.Type;
        question.CategoryId = updatedQuestion.CategoryId;
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
            if (await dbContext.UserAnswers.AnyAsync(answer => answer.QuestionOptionId.HasValue
                && removedOptionIds.Contains(answer.QuestionOptionId.Value), cancellationToken))
                throw new InvalidOperationException("Options referenced by saved answers cannot be removed.");
            foreach (var removedOption in removedOptions)
                question.Options.Remove(removedOption);
            dbContext.QuestionOptions.RemoveRange(removedOptions);
        }
    }

    public async Task DeleteQuestionAsync(
        ApplicationUser actor,
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        var actorId = actor.Id;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var question = await dbContext.Questions
            .Include(item => item.Options)
            .SingleOrDefaultAsync(item => item.Id == questionId, cancellationToken)
            ?? throw new KeyNotFoundException("Question was not found.");

        if (await dbContext.UserAnswers.AnyAsync(answer => answer.QuestionId == questionId, cancellationToken))
            throw new InvalidOperationException("Questions referenced by saved answers cannot be deleted.");

        dbContext.AuditEvents.Add(CreateAuditEvent(actorId, "Question.Deleted", question.Id.ToString(), new { question.Code }));
        dbContext.Questions.Remove(question);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
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
