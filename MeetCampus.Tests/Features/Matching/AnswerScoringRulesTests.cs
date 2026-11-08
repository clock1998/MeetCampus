using MeetCampus.Features.Matching.Services;
using Xunit;

namespace MeetCampus.Tests.Features.Matching;

public sealed class AnswerScoringRulesTests
{
    [Fact]
    public void WhenSingleChoicesMatchThenScoreIsOneHundred()
    {
        var option = Guid.NewGuid();

        var score = AnswerScoringRuleService.ScoreSingleChoice(option, option);

        Assert.Equal(100m, score);
    }

    [Fact]
    public void WhenSingleChoicesDifferThenScoreIsZero()
    {
        var score = AnswerScoringRuleService.ScoreSingleChoice(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(0m, score);
    }

    [Fact]
    public void WhenSingleChoiceIsEmptyThenItIsRejected()
    {
        Assert.Throws<ArgumentException>(() => AnswerScoringRuleService.ScoreSingleChoice(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void WhenMultipleChoicesPartiallyOverlapThenScoreUsesDiceCoefficient()
    {
        var shared = Guid.NewGuid();
        var first = new[] { shared, Guid.NewGuid() };
        var second = new[] { shared, Guid.NewGuid() };

        var score = AnswerScoringRuleService.ScoreMultipleChoice(first, second);

        Assert.Equal(50m, score);
    }

    [Fact]
    public void WhenOneSelectionIsContainedInTheOtherThenScoreUsesBothSelectionCounts()
    {
        var shared = Guid.NewGuid();
        var first = new[] { shared };
        var second = new[] { shared, Guid.NewGuid(), Guid.NewGuid() };

        var score = AnswerScoringRuleService.ScoreMultipleChoice(first, second);

        Assert.Equal(50m, score);
    }

    [Fact]
    public void WhenMultipleChoicesAreIdenticalInDifferentOrderThenScoreIsOneHundred()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        var score = AnswerScoringRuleService.ScoreMultipleChoice([first, second], [second, first]);

        Assert.Equal(100m, score);
    }

    [Fact]
    public void WhenMultipleChoicesDoNotOverlapThenScoreIsZero()
    {
        var score = AnswerScoringRuleService.ScoreMultipleChoice([Guid.NewGuid()], [Guid.NewGuid()]);

        Assert.Equal(0m, score);
    }

    [Fact]
    public void WhenMultipleChoiceIsEmptyThenItIsRejected()
    {
        Assert.Throws<ArgumentException>(() => AnswerScoringRuleService.ScoreMultipleChoice([], [Guid.NewGuid()]));
    }

    [Fact]
    public void WhenMultipleChoiceContainsDuplicatesThenItIsRejected()
    {
        var option = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => AnswerScoringRuleService.ScoreMultipleChoice([option, option], [option]));
    }

    [Fact]
    public void WhenMultipleChoiceContainsEmptyIdThenItIsRejected()
    {
        Assert.Throws<ArgumentException>(() => AnswerScoringRuleService.ScoreMultipleChoice([Guid.Empty], [Guid.NewGuid()]));
    }

    [Fact]
    public void WhenMultipleChoiceIsNullThenItIsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => AnswerScoringRuleService.ScoreMultipleChoice(null!, [Guid.NewGuid()]));
    }

    [Theory]
    [InlineData(4, 3, 1, 5, 75)]
    [InlineData(3, 3, 1, 5, 100)]
    [InlineData(1, 5, 1, 5, 0)]
    [InlineData(-2, 1, -2, 2, 25)]
    public void WhenScaleAnswersAreComparedThenScoreUsesNormalizedDistance(
        int first, int second, int minimum, int maximum, int expected)
    {
        var score = AnswerScoringRuleService.ScoreScale(first, second, minimum, maximum);

        Assert.Equal((decimal)expected, score);
    }

    [Fact]
    public void WhenScaleRangeIsInvalidThenItIsRejected()
    {
        Assert.Throws<ArgumentException>(() => AnswerScoringRuleService.ScoreScale(1, 1, 1, 1));
    }

    [Fact]
    public void WhenScaleAnswerIsOutsideRangeThenItIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AnswerScoringRuleService.ScoreScale(0, 3, 1, 5));
    }

    [Fact]
    public void WhenScaleUsesFullIntegerRangeThenDistanceDoesNotOverflow()
    {
        var score = AnswerScoringRuleService.ScoreScale(int.MinValue, int.MaxValue, int.MinValue, int.MaxValue);

        Assert.Equal(0m, score);
    }
}
