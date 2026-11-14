using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Features.Matching.Models;
using MeetCampus.Features.Matching.Services;
using Xunit;

namespace MeetCampus.Tests.Features.Matching;

public sealed class CompatibilityScoringServiceTests
{
    [Fact]
    public void WhenEveryCategoryHasAnswersThenOverallScoreUsesCategoryWeights()
    {
        var questions = AllCategories();
        var result = CompatibilityScoringService.Calculate(CreateInput(questions));

        Assert.Equal(100m, result.Score);
    }

    [Fact]
    public void WhenScaleAnswersDifferThenDecimalScoreIsPreserved()
    {
        var questions = AllCategories();
        questions[0] = new ScoringQuestion(Guid.NewGuid(), QuestionType.Scale, "interests", 0.5m, 1, 4,
            new ScoringAnswer([], 1), new ScoringAnswer([], 2));

        var result = CompatibilityScoringService.Calculate(CreateInput(questions));

        Assert.Equal(75m + (100m * (1m - 1m / 3m) * 0.25m), result.Score);
    }

    [Fact]
    public void WhenQuestionIsMissingAnAnswerThenScoringIsRejected()
    {
        var questions = AllCategories();
        questions[0] = questions[0] with { SecondAnswer = null };

        Assert.Throws<ArgumentException>(() =>
            CompatibilityScoringService.Calculate(CreateInput(questions)));
    }

    [Fact]
    public void WhenNoQuestionsCanBeComparedThenCoverageIsZero()
    {
        var result = CompatibilityScoringService.Calculate(CreateInput([]));

        Assert.Equal(0m, result.EvidenceCoverage);
    }

    [Fact]
    public void WhenNoQuestionsCanBeComparedThenOverallScoreIsUnavailable()
    {
        var result = CompatibilityScoringService.Calculate(CreateInput([]));

        Assert.Null(result.Score);
    }

    [Fact]
    public void WhenAnyQuestionIsMissingAnAnswerThenScoringIsRejected()
    {
        var questions = AllCategories();
        var missing = questions[0] with { Id = Guid.NewGuid(), SecondAnswer = null };

        Assert.Throws<ArgumentException>(() =>
            CompatibilityScoringService.Calculate(CreateInput([questions[0], missing])));
    }

    [Fact]
    public void WhenCategoryWeightIsZeroThenMissingCategoryDoesNotPreventOverallScore()
    {
        var questions = AllCategories().Where(question => question.Category != "interests").ToArray();
        var categories = Categories()
            .Select(category => category with
            {
                MatchingWeight = category.Code == "values" ? 1m : 0m
            })
            .ToArray();

        var result = CompatibilityScoringService.Calculate(CreateInput(questions, categories));

        Assert.Equal(100m, result.Score);
    }

    [Fact]
    public void WhenCategoryHasNoQuestionsThenItsWeightIsExcludedFromCoverage()
    {
        var questions = AllCategories().Where(question => question.Category == "values").ToArray();

        var result = CompatibilityScoringService.Calculate(CreateInput(questions));

        Assert.Equal(1m, result.EvidenceCoverage);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1.1)]
    public void WhenQuestionWeightIsOutsideExclusiveBoundsThenScoringIsRejected(double weight)
    {
        var questions = AllCategories();
        questions[0] = questions[0] with { ScoringWeight = (decimal)weight };

        Assert.Throws<ArgumentOutOfRangeException>(() => CompatibilityScoringService.Calculate(CreateInput(questions)));
    }

    [Fact]
    public void WhenCategoryHasNoQuestionsThenItIsOmitted()
    {
        var questions = AllCategories().Where(question => question.Category == "values").ToArray();

        var result = CompatibilityScoringService.Calculate(CreateInput(questions));

        Assert.Single(result.Categories, category => category.Category == "values");
    }

    [Fact]
    public void WhenCategoriesAreEmptyThenRemainingWeightsAreNormalized()
    {
        var questions = AllCategories().Where(question => question.Category is "interests" or "values").ToArray();
        questions[0] = questions[0] with { SecondAnswer = new ScoringAnswer([Guid.NewGuid()], null) };

        var result = CompatibilityScoringService.Calculate(CreateInput(questions));

        Assert.Equal(100m * 0.30m / 0.55m, result.Score);
    }

    [Fact]
    public void WhenPopulatedCategoriesHaveNoMatchingWeightThenOverallScoreIsUnavailable()
    {
        var questions = AllCategories().Where(question => question.Category == "values").ToArray();
        var categories = Categories().Select(category => category with { MatchingWeight = 0m }).ToArray();

        var result = CompatibilityScoringService.Calculate(CreateInput(questions, categories));

        Assert.Null(result.Score);
    }

    [Fact]
    public void WhenQuestionsHaveDifferentWeightsThenCategoryUsesWeightedAverage()
    {
        var questions = AllCategories().ToList();
        var option = Guid.NewGuid();
        questions.Add(new ScoringQuestion(Guid.NewGuid(), QuestionType.SingleChoice, "interests", 0.5m, null, null,
            new ScoringAnswer([option], null), new ScoringAnswer([Guid.NewGuid()], null)));
        questions[0] = questions[0] with { ScoringWeight = 0.5m };

        var result = CompatibilityScoringService.Calculate(CreateInput(questions));

        Assert.Equal(50m, result.Categories.Single(category => category.Category == "interests").Score);
    }

    [Fact]
    public void WhenQuestionOrderChangesThenResultIsUnchanged()
    {
        var questions = AllCategories();

        var first = CompatibilityScoringService.Calculate(CreateInput(questions));
        var second = CompatibilityScoringService.Calculate(CreateInput(questions.Reverse().ToArray()));

        Assert.Equal(first.Score, second.Score);
    }

    private static ScoringInput CreateInput(
        IReadOnlyList<ScoringQuestion> questions,
        IReadOnlyList<ScoringCategory>? categories = null) =>
        new(categories ?? Categories(), questions, [], []);

    private static ScoringCategory[] Categories() =>
    [
        new("interests", 0.25m),
        new("values", 0.30m),
        new("social", 0.20m),
        new("lifestyle", 0.15m),
        new("connection-intent", 0.10m)
    ];

    private static ScoringQuestion[] AllCategories() =>
        new[] { "interests", "values", "social", "lifestyle", "connection-intent" }
            .Select(category =>
            {
                var option = Guid.NewGuid();
                return new ScoringQuestion(Guid.NewGuid(), QuestionType.SingleChoice, category, 0.5m, null, null,
                    new ScoringAnswer([option], null), new ScoringAnswer([option], null));
            }).ToArray();
}
