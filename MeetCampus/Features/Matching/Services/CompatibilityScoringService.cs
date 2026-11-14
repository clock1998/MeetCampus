using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Features.Matching.Models;

namespace MeetCampus.Features.Matching.Services;

public static class CompatibilityScoringService
{
    /// <summary>Calculates category scores, overall compatibility, and answer coverage.</summary>
    public static CompatibilityScoreResult Calculate(ScoringInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Categories);
        ArgumentNullException.ThrowIfNull(input.Questions);

        var categories = input.Categories.ToDictionary(category => category.Code, StringComparer.Ordinal);
        if (input.Categories.Any(category => category.MatchingWeight < 0))
            throw new ArgumentOutOfRangeException(nameof(input), "Category weights must not be negative.");
        foreach (var question in input.Questions)
        {
            if (!categories.ContainsKey(question.Category))
                throw new ArgumentException("Question category is not present in the scoring categories.", nameof(input));
            if (question.ScoringWeight is <= 0 or >= 1)
                throw new ArgumentOutOfRangeException(nameof(input), "Question weights must be greater than zero and less than one.");
            if (question.FirstAnswer is null || question.SecondAnswer is null)
                throw new ArgumentException("Compatibility scoring requires an answer from both members for every question.", nameof(input));
        }

        var results = categories.Values
            .Select(category => (category.Code, Questions: input.Questions.Where(question => question.Category == category.Code).ToArray()))
            .Where(category => category.Questions.Length > 0)
            .Select(category => CalculateScoreByCategory(category.Code, category.Questions))
            .ToArray();
        var totalWeight = results.Sum(category => categories[category.Category].MatchingWeight);
        var coverage = totalWeight == 0 ? 0 : 1m;
        var score = totalWeight == 0
            ? (decimal?)null
            : results.Sum(category => categories[category.Category].MatchingWeight * category.Score) / totalWeight;

        return new CompatibilityScoreResult(score, coverage, results);
    }

    private static CategoryScore CalculateScoreByCategory(string category, IReadOnlyList<ScoringQuestion> questions)
    {
        var weight = questions.Sum(question => question.ScoringWeight);
        var score = questions.Sum(question => ScoreQuestion(question) * question.ScoringWeight) / weight;
        return new CategoryScore(category, score, questions.Count);
    }

    private static decimal ScoreQuestion(ScoringQuestion question)
    {
        var first = question.FirstAnswer!;
        var second = question.SecondAnswer!;
        return question.Type switch
        {
            QuestionType.SingleChoice when first.OptionIds.Count == 1 && second.OptionIds.Count == 1
                && first.ScaleValue is null && second.ScaleValue is null =>
                AnswerScoringRuleService.ScoreSingleChoice(first.OptionIds[0], second.OptionIds[0]),
            QuestionType.MultipleChoice when first.ScaleValue is null && second.ScaleValue is null =>
                AnswerScoringRuleService.ScoreMultipleChoice(first.OptionIds, second.OptionIds),
            QuestionType.Scale when first.OptionIds.Count == 0 && second.OptionIds.Count == 0
                && first.ScaleValue.HasValue && second.ScaleValue.HasValue && question.ScaleMin.HasValue && question.ScaleMax.HasValue =>
                AnswerScoringRuleService.ScoreScale(first.ScaleValue.Value, second.ScaleValue.Value, question.ScaleMin.Value, question.ScaleMax.Value),
            _ => throw new ArgumentException("The question contains an unsupported answer shape.", nameof(question))
        };
    }
}
