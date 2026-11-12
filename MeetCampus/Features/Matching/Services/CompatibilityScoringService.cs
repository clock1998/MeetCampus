using MeetCampus.Configuration;
using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Features.Matching.Models;

namespace MeetCampus.Features.Matching.Services;

public static class CompatibilityScoringService
{
    private static readonly string[] Categories = ["interests", "values", "social", "lifestyle", "connection-intent"];

    /// <summary>Calculates category scores, overall compatibility, and answer coverage.</summary>
    public static CompatibilityScoreResult Calculate(ScoringInput input, MatchingWeightsOptions weights)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(weights);
        ArgumentNullException.ThrowIfNull(input.Questions);

        foreach (var question in input.Questions)
        {
            weights.GetCategoryWeight(question.Category);
            if (question.ScoringWeight is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(input), "Question weights must be between zero and one.");
            if (question.FirstAnswer is null || question.SecondAnswer is null)
                throw new ArgumentException("Compatibility scoring requires an answer from both members for every question.", nameof(input));
        }

        var results = Categories.Select(category => CalculateCategory(category, input.Questions)).ToArray();
        var populated = results.Where(category => category.QuestionCount > 0).ToArray();
        var coverageWeight = populated.Sum(category => weights.GetCategoryWeight(category.Category));
        var coverage = coverageWeight == 0 ? 0 : 1m;
        var score = results.Any(category => weights.GetCategoryWeight(category.Category) > 0 && category.Score is null)
            ? (decimal?)null
            : results.Sum(category => weights.GetCategoryWeight(category.Category) * (category.Score ?? 0));

        return new CompatibilityScoreResult(score, coverage, results);
    }

    private static CategoryScore CalculateCategory(string category, IReadOnlyList<ScoringQuestion> questions)
    {
        var categoryQuestions = questions.Where(question => question.Category == category).ToArray();
        var weight = categoryQuestions.Sum(question => question.ScoringWeight);
        var score = weight == 0 ? (decimal?)null : categoryQuestions.Sum(question => ScoreQuestion(question) * question.ScoringWeight) / weight;
        return new CategoryScore(category, score, categoryQuestions.Length);
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
