using MeetCampus.Features.Matching.Models;

namespace MeetCampus.Features.Matching.Services;

public static class MatchExplanationService
{
    /// <summary>Creates a match label and generic category explanations without exposing scores or answers.</summary>
    public static MatchExplanationResult Explain(CompatibilityScoreResult score, IReadOnlyList<ScoringCategory> categories)
    {
        ArgumentNullException.ThrowIfNull(score);
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(score.Categories);

        if (score.Score is null)
            return new MatchExplanationResult(null, false, []);

        if (score.Score is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(score), "The overall score must be between zero and one hundred.");
        if (score.EvidenceCoverage != 1m)
            throw new ArgumentException("A match explanation requires complete answer coverage.", nameof(score));

        var categoriesByCode = categories.ToDictionary(category => category.Code, StringComparer.Ordinal);
        var qualifyingCategories = new List<ScoringCategory>();
        var seenCategories = new HashSet<string>(StringComparer.Ordinal);
        foreach (var categoryScore in score.Categories)
        {
            if (categoryScore.QuestionCount <= 0 || categoryScore.Score is < 0 or > 100)
                throw new ArgumentException("Category scores must be based on questions and between zero and one hundred.", nameof(score));
            if (!seenCategories.Add(categoryScore.Category))
                throw new ArgumentException("Category scores must not contain duplicate categories.", nameof(score));
            if (!categoriesByCode.TryGetValue(categoryScore.Category, out var category))
                throw new ArgumentException("A scored category is missing its metadata.", nameof(categories));
            if (categoryScore.Score >= 75m)
            {
                if (string.IsNullOrWhiteSpace(category.NameEn) || string.IsNullOrWhiteSpace(category.NameFr))
                    throw new ArgumentException("An explanation category requires English and French names.", nameof(categories));
                qualifyingCategories.Add(category);
            }
        }

        var label = score.Score switch
        {
            >= 80m => MatchLabel.Strong,
            >= 60m => MatchLabel.Good,
            _ => MatchLabel.Potential
        };
        var explanations = qualifyingCategories
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Code, StringComparer.Ordinal)
            .Take(3)
            .Select(category => new MatchExplanationCategory(category.NameEn, category.NameFr))
            .ToArray();

        return new MatchExplanationResult(label, true, explanations);
    }
}
