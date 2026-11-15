using MeetCampus.Features.Matching.Models;
using MeetCampus.Features.Matching.Services;
using Xunit;

namespace MeetCampus.Tests.Features.Matching;

public sealed class MatchExplanationServiceTests
{
    [Theory]
    [InlineData(0, MatchLabel.Potential)]
    [InlineData(59.999, MatchLabel.Potential)]
    [InlineData(60, MatchLabel.Good)]
    [InlineData(79.999, MatchLabel.Good)]
    [InlineData(80, MatchLabel.Strong)]
    [InlineData(100, MatchLabel.Strong)]
    public void WhenScoreIsAvailableThenLabelUsesUnroundedThresholds(double value, MatchLabel expected)
    {
        var score = new CompatibilityScoreResult((decimal)value, 1m, []);

        var result = MatchExplanationService.Explain(score, []);

        Assert.Equal(expected, result.Label);
    }

    [Fact]
    public void WhenOverallScoreIsUnavailableThenNoLabelIsReturned()
    {
        var score = new CompatibilityScoreResult(null, 0m, [new CategoryScore("values", 100m, 1)]);

        var result = MatchExplanationService.Explain(score, [new ScoringCategory("values", 1m, "Values", "Valeurs", 0)]);

        Assert.Null(result.Label);
    }

    [Fact]
    public void WhenOverallScoreIsUnavailableThenNoExplanationsAreReturned()
    {
        var score = new CompatibilityScoreResult(null, 0m, [new CategoryScore("values", 100m, 1)]);

        var result = MatchExplanationService.Explain(score, [new ScoringCategory("values", 1m, "Values", "Valeurs", 0)]);

        Assert.Empty(result.Categories);
    }

    [Fact]
    public void WhenScoreIsAvailableThenCoverageIsComplete()
    {
        var score = new CompatibilityScoreResult(60m, 1m, []);

        var result = MatchExplanationService.Explain(score, []);

        Assert.True(result.IsComplete);
    }

    [Fact]
    public void WhenScoreIsAvailableButCoverageIsIncompleteThenExplanationIsRejected()
    {
        var score = new CompatibilityScoreResult(60m, 0.5m, []);

        Assert.Throws<ArgumentException>(() => MatchExplanationService.Explain(score, []));
    }

    [Fact]
    public void WhenCategoryScoresMeetThresholdThenOnlyQualifyingCategoriesAreReturned()
    {
        var categories = Categories();
        var score = new CompatibilityScoreResult(80m, 1m,
            [new CategoryScore("a", 74.999m, 1), new CategoryScore("b", 75m, 1)]);

        var result = MatchExplanationService.Explain(score, categories);

        Assert.Equal([new MatchExplanationCategory("B", "B fr")], result.Categories);
    }

    [Fact]
    public void WhenMoreThanThreeCategoriesQualifyThenSortOrderAndCodeDetermineTheFirstThree()
    {
        var score = new CompatibilityScoreResult(80m, 1m,
            [new CategoryScore("d", 100m, 1), new CategoryScore("c", 75m, 1),
             new CategoryScore("b", 80m, 1), new CategoryScore("a", 90m, 1)]);

        var result = MatchExplanationService.Explain(score, Categories());

        Assert.Equal(["A", "B", "C"], result.Categories.Select(category => category.NameEn));
    }

    [Fact]
    public void WhenCategoryInputOrderChangesThenExplanationsAreUnchanged()
    {
        var score = new CompatibilityScoreResult(80m, 1m,
            [new CategoryScore("b", 75m, 1), new CategoryScore("a", 90m, 1)]);
        var categories = Categories();

        var first = MatchExplanationService.Explain(score, categories);
        var second = MatchExplanationService.Explain(score with { Categories = score.Categories.Reverse().ToArray() }, categories.Reverse().ToArray());

        Assert.Equal(first.Categories, second.Categories);
    }

    [Fact]
    public void WhenNoCategoriesQualifyThenNoExplanationsAreReturned()
    {
        var score = new CompatibilityScoreResult(60m, 1m, [new CategoryScore("a", 74m, 1)]);

        var result = MatchExplanationService.Explain(score, Categories());

        Assert.Empty(result.Categories);
    }

    [Fact]
    public void WhenQualifyingCategoryHasNoApprovedNameThenExplanationIsRejected()
    {
        var score = new CompatibilityScoreResult(80m, 1m, [new CategoryScore("a", 75m, 1)]);
        var categories = new[] { new ScoringCategory("a", 1m, "", "A fr", 0) };

        Assert.Throws<ArgumentException>(() => MatchExplanationService.Explain(score, categories));
    }

    [Fact]
    public void WhenCategoryMetadataIsMissingThenExplanationIsRejected()
    {
        var score = new CompatibilityScoreResult(80m, 1m, [new CategoryScore("missing", 90m, 1)]);

        Assert.Throws<ArgumentException>(() => MatchExplanationService.Explain(score, Categories()));
    }

    private static ScoringCategory[] Categories() =>
    [
        new("d", 0.25m, "D", "D fr", 2),
        new("b", 0.25m, "B", "B fr", 1),
        new("c", 0.25m, "C", "C fr", 1),
        new("a", 0.25m, "A", "A fr", 0)
    ];
}
