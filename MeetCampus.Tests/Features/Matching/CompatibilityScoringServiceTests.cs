using MeetCampus.Configuration;
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
        var result = CompatibilityScoringService.Calculate(new ScoringInput(questions, [], []), new MatchingWeightsOptions());

        Assert.Equal(100m, result.Score);
    }

    [Fact]
    public void WhenScaleAnswersDifferThenDecimalScoreIsPreserved()
    {
        var questions = AllCategories();
        questions[0] = new ScoringQuestion(Guid.NewGuid(), QuestionType.Scale, "interests", 1m, 1, 4,
            new ScoringAnswer([], 1), new ScoringAnswer([], 2));

        var result = CompatibilityScoringService.Calculate(new ScoringInput(questions, [], []), new MatchingWeightsOptions());

        Assert.Equal(75m + (100m * (1m - 1m / 3m) * 0.25m), result.Score);
    }

    [Fact]
    public void WhenQuestionIsMissingAnAnswerThenScoringIsRejected()
    {
        var questions = AllCategories();
        questions[0] = questions[0] with { SecondAnswer = null };

        Assert.Throws<ArgumentException>(() =>
            CompatibilityScoringService.Calculate(new ScoringInput(questions, [], []), new MatchingWeightsOptions()));
    }

    [Fact]
    public void WhenNoQuestionsCanBeComparedThenCoverageIsZero()
    {
        var result = CompatibilityScoringService.Calculate(new ScoringInput([], [], []), new MatchingWeightsOptions());

        Assert.Equal(0m, result.EvidenceCoverage);
    }

    [Fact]
    public void WhenNoQuestionsCanBeComparedThenOverallScoreIsUnavailable()
    {
        var result = CompatibilityScoringService.Calculate(new ScoringInput([], [], []), new MatchingWeightsOptions());

        Assert.Null(result.Score);
    }

    [Fact]
    public void WhenAnyQuestionIsMissingAnAnswerThenScoringIsRejected()
    {
        var questions = AllCategories();
        var missing = questions[0] with { Id = Guid.NewGuid(), SecondAnswer = null };

        Assert.Throws<ArgumentException>(() =>
            CompatibilityScoringService.Calculate(new ScoringInput([questions[0], missing], [], []), new MatchingWeightsOptions()));
    }

    [Fact]
    public void WhenCategoryWeightIsZeroThenMissingCategoryDoesNotPreventOverallScore()
    {
        var questions = AllCategories().Where(question => question.Category != "interests").ToArray();
        var weights = new MatchingWeightsOptions { Interests = 0m, Values = 0.55m };

        var result = CompatibilityScoringService.Calculate(new ScoringInput(questions, [], []), weights);

        Assert.Equal(100m, result.Score);
    }

    [Fact]
    public void WhenCategoryHasNoQuestionsThenItsWeightIsExcludedFromCoverage()
    {
        var questions = AllCategories().Where(question => question.Category == "values").ToArray();

        var result = CompatibilityScoringService.Calculate(new ScoringInput(questions, [], []), new MatchingWeightsOptions());

        Assert.Equal(1m, result.EvidenceCoverage);
    }

    [Fact]
    public void WhenAllComparableQuestionWeightsAreZeroThenCategoryHasNoScore()
    {
        var questions = AllCategories();
        questions[0] = questions[0] with { ScoringWeight = 0m };

        var result = CompatibilityScoringService.Calculate(new ScoringInput(questions, [], []), new MatchingWeightsOptions());

        Assert.Null(result.Categories.Single(category => category.Category == "interests").Score);
    }

    [Fact]
    public void WhenQuestionsHaveDifferentWeightsThenCategoryUsesWeightedAverage()
    {
        var questions = AllCategories().ToList();
        var option = Guid.NewGuid();
        questions.Add(new ScoringQuestion(Guid.NewGuid(), QuestionType.SingleChoice, "interests", 0.5m, null, null,
            new ScoringAnswer([option], null), new ScoringAnswer([Guid.NewGuid()], null)));
        questions[0] = questions[0] with { ScoringWeight = 0.5m };

        var result = CompatibilityScoringService.Calculate(new ScoringInput(questions, [], []), new MatchingWeightsOptions());

        Assert.Equal(50m, result.Categories.Single(category => category.Category == "interests").Score);
    }

    [Fact]
    public void WhenQuestionOrderChangesThenResultIsUnchanged()
    {
        var questions = AllCategories();
        var weights = new MatchingWeightsOptions();

        var first = CompatibilityScoringService.Calculate(new ScoringInput(questions, [], []), weights);
        var second = CompatibilityScoringService.Calculate(new ScoringInput(questions.Reverse().ToArray(), [], []), weights);

        Assert.Equal(first.Score, second.Score);
    }

    private static ScoringQuestion[] AllCategories() =>
        new[] { "interests", "values", "social", "lifestyle", "connection-intent" }
            .Select(category =>
            {
                var option = Guid.NewGuid();
                return new ScoringQuestion(Guid.NewGuid(), QuestionType.SingleChoice, category, 1m, null, null,
                    new ScoringAnswer([option], null), new ScoringAnswer([option], null));
            }).ToArray();
}
