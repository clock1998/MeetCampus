using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Contracts.Features.Questionnaire;
using MeetCampus.Data;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.User;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Features.Questionnaire.Services;

public sealed class QuestionnaireResponseService(ApplicationDbContext dbContext)
{
    public async Task<QuestionnaireResponse> GetResponseAsync(
        ApplicationUser member,
        string cultureCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(member);
        var memberId = member.Id;
        ValidateCulture(cultureCode);

        var questions = await dbContext.Questions
            .AsNoTracking()
            .Include(question => question.Translations)
            .Include(question => question.Options)
                .ThenInclude(option => option.Translations)
            .OrderBy(question => question.SortOrder)
            .ThenBy(question => question.Code)
            .ToListAsync(cancellationToken);
        var answers = await dbContext.UserAnswers
            .AsNoTracking()
            .Where(answer => answer.ApplicationUserId == memberId)
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
                && IsValidAnswer(question, questionAnswers);

            return new QuestionnaireQuestionResponse(
                question.Id,
                question.Code,
                (QuestionType)question.Type,
                prompt,
                question.IsRequired,
                question.Group,
                question.ScaleMin,
                question.ScaleMax,
                options,
                questionAnswers.Where(answer => answer.QuestionOption is not null).Select(answer => answer.QuestionOption!.OptionCode).OrderBy(code => code, StringComparer.Ordinal).ToList(),
                questionAnswers.Select(answer => answer.ScaleValue).SingleOrDefault(value => value.HasValue),
                questionAnswers.Count > 0 && !validCurrentAnswer);
        }).ToList();

        var requiredQuestions = questions.Where(question => question.IsRequired).ToList();
        var responseById = questionResponses.ToDictionary(question => question.Id);
        var isComplete = requiredQuestions.All(question =>
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
        var memberId = member.Id;
        ArgumentNullException.ThrowIfNull(request.Answers);

        if (request.Answers.Select(answer => answer.QuestionId).Distinct().Count() != request.Answers.Count)
            throw new ArgumentException("Each question may appear only once in a questionnaire submission.", nameof(request));

        var questions = await dbContext.Questions
            .Include(question => question.Options)
            .ToDictionaryAsync(question => question.Id, cancellationToken);

        foreach (var answer in request.Answers)
        {
            if (!questions.TryGetValue(answer.QuestionId, out var question))
                throw new ArgumentException("The submission contains an unknown question.", nameof(request));
            ValidateAnswer(question, answer);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var questionIds = request.Answers.Select(answer => answer.QuestionId).ToArray();
        var existingAnswers = await dbContext.UserAnswers
            .Where(answer => answer.ApplicationUserId == memberId && questionIds.Contains(answer.QuestionId))
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
                        ApplicationUserId = memberId,
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
                    ApplicationUserId = memberId,
                    QuestionId = question.Id,
                    ScaleValue = answer.ScaleValue,
                    UpdatedAt = updatedAt
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static void ValidateAnswer(Question question, QuestionnaireAnswerRequest answer)
    {
        if (answer.OptionCodes is null)
            throw new ArgumentException("Option selections cannot be null.", nameof(answer));

        if (answer.OptionCodes.Distinct(StringComparer.Ordinal).Count() != answer.OptionCodes.Count)
            throw new ArgumentException("An option can only be selected once per question.", nameof(answer));

        switch ((QuestionType)question.Type)
        {
            case QuestionType.SingleChoice:
                if (answer.ScaleValue.HasValue || answer.OptionCodes.Count > 1)
                    throw new ArgumentException("Single-choice questions allow at most one option selection.", nameof(answer));
                ValidateOptions(question, answer.OptionCodes);
                break;
            case QuestionType.MultipleChoice:
                if (answer.ScaleValue.HasValue
                    || question.MaxSelections.HasValue && answer.OptionCodes.Count > question.MaxSelections.Value)
                    throw new ArgumentException("The multiple-choice answer has an invalid number of selections.", nameof(answer));
                ValidateOptions(question, answer.OptionCodes);
                break;
            case QuestionType.Scale:
                if (answer.OptionCodes.Count > 0 || answer.ScaleValue.HasValue
                    && (answer.ScaleValue < question.ScaleMin || answer.ScaleValue > question.ScaleMax))
                    throw new ArgumentException("The scale answer is outside the configured range.", nameof(answer));
                break;
            default:
                throw new ArgumentException("The question type is not supported.", nameof(question));
        }
    }

    private static void ValidateOptions(Question question, IReadOnlyList<string> optionCodes)
    {
        var validCodes = question.Options.Select(option => option.OptionCode).ToHashSet(StringComparer.Ordinal);
        if (optionCodes.Any(code => !validCodes.Contains(code)))
            throw new ArgumentException("The answer contains an option that does not belong to the question.", nameof(optionCodes));
    }

    private static bool IsValidAnswer(Question question, IReadOnlyList<UserAnswer> answers)
    {
        if (answers.Count == 0)
            return false;

        return (QuestionType)question.Type switch
        {
            QuestionType.SingleChoice => answers.Count == 1 && answers[0].QuestionOptionId.HasValue && answers[0].ScaleValue is null
                && question.Options.Any(option => option.Id == answers[0].QuestionOptionId),
            QuestionType.MultipleChoice => answers.All(answer => answer.QuestionOptionId.HasValue && answer.ScaleValue is null
                && question.Options.Any(option => option.Id == answer.QuestionOptionId))
                && (!question.MaxSelections.HasValue || answers.Count <= question.MaxSelections.Value),
            QuestionType.Scale => answers.Count == 1 && answers[0].QuestionOptionId is null && answers[0].ScaleValue is int value
                && value >= question.ScaleMin && value <= question.ScaleMax,
            _ => false
        };
    }

    private static void ValidateCulture(string cultureCode)
    {
        if (cultureCode is not ("en" or "fr"))
            throw new ArgumentException("Supported questionnaire cultures are 'en' and 'fr'.", nameof(cultureCode));
    }
}
