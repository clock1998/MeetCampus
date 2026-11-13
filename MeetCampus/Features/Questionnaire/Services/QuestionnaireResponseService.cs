using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Contracts.Features.Questionnaire;
using MeetCampus.Data;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.User;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Features.Questionnaire.Services;

public sealed class QuestionnaireResponseService(
    ApplicationDbContext dbContext,
    IValidator<string> cultureValidator,
    IValidator<SubmitQuestionnaireResponseRequest> requestValidator,
    IValidator<QuestionnaireAnswerValidationContext> answerValidator)
{
    public async Task<QuestionnaireResponse> GetResponseAsync(
        ApplicationUser actor,
        string cultureCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        var cultureValidationResult = await cultureValidator.ValidateAsync(cultureCode, cancellationToken);
        if (!cultureValidationResult.IsValid)
            throw new ArgumentException(string.Join(" ", cultureValidationResult.Errors.Select(error => error.ErrorMessage)), nameof(cultureCode));

        var questions = await dbContext.Questions
            .AsNoTracking()
            .Include(question => question.Category)
            .Include(question => question.Translations)
            .Include(question => question.Options)
                .ThenInclude(option => option.Translations)
            .OrderBy(question => question.SortOrder)
            .ThenBy(question => question.Code)
            .ToListAsync(cancellationToken);
        var answers = await dbContext.UserAnswers
            .AsNoTracking()
            .Where(answer => answer.ApplicationUserId == actor.Id)
            .Include(answer => answer.QuestionOption)
            .ToListAsync(cancellationToken);
        var answerGroups = answers.GroupBy(answer => answer.QuestionId).ToDictionary(group => group.Key, group => group.ToList());

        var questionResponses = questions.Select(question =>
        {
            var questionAnswers = answerGroups.GetValueOrDefault(question.Id, []);
            var prompt = question.Translations.SingleOrDefault(item => item.CultureCode == cultureCode)?.Text ?? string.Empty;
            var options = question.Options.OrderBy(option => option.SortOrder).Select(option => new QuestionnaireOptionResponse(
                option.OptionCode,
                option.Translations.SingleOrDefault(item => item.CultureCode == cultureCode)?.Text ?? string.Empty,
                option.SortOrder)).ToList();
            var validCurrentAnswer = questionAnswers.All(answer => answer.UpdatedAt >= question.UpdatedAt)
                && QuestionnaireAnswerRules.IsValidStoredAnswer(question, questionAnswers);

            return new QuestionnaireQuestionResponse(
                question.Id,
                question.Code,
                (QuestionType)question.Type,
                prompt,
                question.Category.Code,
                question.ScaleMin,
                question.ScaleMax,
                options,
                questionAnswers.Where(answer => answer.QuestionOption is not null).Select(answer => answer.QuestionOption!.OptionCode).OrderBy(code => code, StringComparer.Ordinal).ToList(),
                questionAnswers.Select(answer => answer.ScaleValue).SingleOrDefault(value => value.HasValue),
                questionAnswers.Count > 0 && !validCurrentAnswer);
        }).ToList();

        var responseById = questionResponses.ToDictionary(question => question.Id);
        var isComplete = questions.All(question =>
            responseById.TryGetValue(question.Id, out var response)
            && !response.IsRefreshRequired
            && (response.SelectedOptionCodes.Count > 0 || response.ScaleValue.HasValue));

        return new QuestionnaireResponse(questionResponses, isComplete);
    }

    public async Task SubmitAsync(
        ApplicationUser member,
        SubmitQuestionnaireResponseRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(member);
        var actorId = member.Id;
        var requestValidationResult = await requestValidator.ValidateAsync(request, cancellationToken);
        if (!requestValidationResult.IsValid)
            throw new ArgumentException(string.Join(" ", requestValidationResult.Errors.Select(error => error.ErrorMessage)), nameof(request));

        var questions = await dbContext.Questions
            .Include(question => question.Options)
            .ToDictionaryAsync(question => question.Id, cancellationToken);

        foreach (var answer in request.Answers)
        {
            if (!questions.TryGetValue(answer.QuestionId, out var question))
                throw new ArgumentException("The submission contains an unknown question.", nameof(request));
            var answerValidationResult = await answerValidator.ValidateAsync(
                new QuestionnaireAnswerValidationContext(question, answer), cancellationToken);
            if (!answerValidationResult.IsValid)
                throw new ArgumentException(string.Join(" ", answerValidationResult.Errors.Select(error => error.ErrorMessage)), nameof(request));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var questionIds = request.Answers.Select(answer => answer.QuestionId).ToArray();
        var existingAnswers = await dbContext.UserAnswers
            .Where(answer => answer.ApplicationUserId == actorId && questionIds.Contains(answer.QuestionId))
            .ToListAsync(cancellationToken);
        dbContext.UserAnswers.RemoveRange(existingAnswers);
        var updatedAt = DateTimeOffset.UtcNow;
        foreach (var answer in request.Answers)
        {
            var question = questions[answer.QuestionId];
            var selectedOptions = answer.OptionCodes.Count == 0
                ? []
                : question.Options.Where(option => answer.OptionCodes.Contains(option.OptionCode, StringComparer.Ordinal)).ToList();

            if (selectedOptions.Count > 0)
            {
                foreach (var option in selectedOptions)
                {
                    dbContext.UserAnswers.Add(new UserAnswer
                    {
                        Id = Guid.NewGuid(),
                        ApplicationUserId = actorId,
                        QuestionId = question.Id,
                        QuestionOptionId = option.Id,
                        UpdatedAt = updatedAt
                    });
                }
            }
            else if (answer.ScaleValue.HasValue)
            {
                dbContext.UserAnswers.Add(new UserAnswer
                {
                    Id = Guid.NewGuid(),
                    ApplicationUserId = actorId,
                    QuestionId = question.Id,
                    ScaleValue = answer.ScaleValue,
                    UpdatedAt = updatedAt
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

}
