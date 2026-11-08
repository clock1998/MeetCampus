using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Data;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Features.Matching.Models;
using MeetCampus.Features.Questionnaire.Services;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Features.Matching.Services;

public sealed class ScoringInputService(ApplicationDbContext dbContext)
{
    /// <summary>Loads current scoring questions, usable answers, and intents for two members.</summary>
    public async Task<ScoringInput> LoadAsync(Guid firstUserId, Guid secondUserId, CancellationToken cancellationToken = default)
    {
        if (firstUserId == Guid.Empty)
            throw new ArgumentException("A user ID is required.", nameof(firstUserId));
        if (secondUserId == Guid.Empty)
            throw new ArgumentException("A user ID is required.", nameof(secondUserId));

        var questions = await dbContext.Questions
            .AsNoTracking()
            .Include(question => question.Options)
            .OrderBy(question => question.SortOrder)
            .ToListAsync(cancellationToken);
        var answers = await dbContext.UserAnswers
            .AsNoTracking()
            .Where(answer => answer.ApplicationUserId == firstUserId || answer.ApplicationUserId == secondUserId)
            .ToListAsync(cancellationToken);
        var intentions = await dbContext.UserProfileIntentions
            .AsNoTracking()
            .Where(intention => intention.UserProfile.ApplicationUserId == firstUserId
                || intention.UserProfile.ApplicationUserId == secondUserId)
            .Select(intention => new { UserId = intention.UserProfile.ApplicationUserId, IntentionId = intention.UserIntentionId })
            .ToListAsync(cancellationToken);

        var answersByUserAndQuestion = answers
            .GroupBy(answer => (answer.ApplicationUserId, answer.QuestionId))
            .ToDictionary(group => group.Key, group => (IReadOnlyCollection<UserAnswer>)group.ToArray());
        var scoringQuestions = questions
            .Where(IsSupportedQuestion)
            .Select(question => new ScoringQuestion(
                question.Id,
                question.Type,
                question.Category,
                question.ScoringWeight,
                question.ScaleMin,
                question.ScaleMax,
                GetAnswer(question, firstUserId, answersByUserAndQuestion),
                GetAnswer(question, secondUserId, answersByUserAndQuestion)))
            .ToArray();

        return new ScoringInput(
            scoringQuestions,
            intentions.Where(item => item.UserId == firstUserId).Select(item => item.IntentionId).Order().ToArray(),
            intentions.Where(item => item.UserId == secondUserId).Select(item => item.IntentionId).Order().ToArray());
    }

    private static bool IsSupportedQuestion(Question question) => question.Type switch
    {
        QuestionType.SingleChoice or QuestionType.MultipleChoice => true,
        QuestionType.Scale => question.ScaleMin.HasValue && question.ScaleMax.HasValue && question.ScaleMin < question.ScaleMax,
        _ => false
    };

    private static ScoringAnswer? GetAnswer(
        Question question,
        Guid userId,
        IReadOnlyDictionary<(Guid UserId, Guid QuestionId), IReadOnlyCollection<UserAnswer>> answersByUserAndQuestion)
    {
        if (!answersByUserAndQuestion.TryGetValue((userId, question.Id), out var answers)
            || answers.Any(answer => answer.UpdatedAt < question.UpdatedAt)
            || !QuestionnaireAnswerRules.IsValidStoredAnswer(question, answers))
            return null;

        var optionIds = answers.Where(answer => answer.QuestionOptionId.HasValue)
            .Select(answer => answer.QuestionOptionId!.Value)
            .Order()
            .ToArray();
        if (optionIds.Any(id => id == Guid.Empty) || optionIds.Distinct().Count() != optionIds.Length)
            return null;

        return new ScoringAnswer(optionIds, question.Type == QuestionType.Scale ? answers.Single().ScaleValue : null);
    }
}
