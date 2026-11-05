using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Data.Questionnaire;

namespace MeetCampus.Features.Questionnaire.Services;

internal static class QuestionnaireAnswerRules
{
    internal static bool IsValidStoredAnswer(Question question, IReadOnlyCollection<UserAnswer> answers)
    {
        if (answers.Count == 0)
            return false;

        return (QuestionType)question.Type switch
        {
            QuestionType.SingleChoice => answers.Count == 1
                && answers.All(answer => answer.QuestionOptionId.HasValue && answer.ScaleValue is null
                    && question.Options.Any(option => option.Id == answer.QuestionOptionId)),
            QuestionType.MultipleChoice => answers.All(answer => answer.QuestionOptionId.HasValue && answer.ScaleValue is null
                && question.Options.Any(option => option.Id == answer.QuestionOptionId))
                && (!question.MaxSelections.HasValue || answers.Count <= question.MaxSelections.Value),
            QuestionType.Scale => answers.Count == 1 && answers.All(answer => answer.QuestionOptionId is null
                && answer.ScaleValue is int value && value >= question.ScaleMin && value <= question.ScaleMax),
            _ => false
        };
    }
}
