using MeetCampus.Features.Matching.Services;
using Xunit;

namespace MeetCampus.Tests.Features.Matching;

public sealed class IntentCompatibilityServiceTests
{
    [Theory]
    [InlineData("friendship")]
    [InlineData("dating")]
    [InlineData("study-partner")]
    [InlineData("activity-partner")]
    public void WhenBothMembersSelectSameIntentThenTheyAreCompatible(string intent)
    {
        Assert.True(IntentCompatibilityService.AreCompatible([intent], [intent]));
    }

    [Theory]
    [InlineData("friendship", "study-partner")]
    [InlineData("dating", "friendship")]
    [InlineData("friendship", "dating")]
    public void WhenMembersSelectDifferentIntentsThenTheyAreNotCompatible(string first, string second)
    {
        Assert.False(IntentCompatibilityService.AreCompatible([first], [second]));
    }

    [Fact]
    public void WhenAnySelectedPairMatchesThenMembersAreCompatible()
    {
        Assert.True(IntentCompatibilityService.AreCompatible(["dating", "friendship"], ["study-partner", "friendship"]));
    }

    [Fact]
    public void WhenNoIntentsAreSelectedThenMembersAreNotCompatible()
    {
        Assert.False(IntentCompatibilityService.AreCompatible([], ["friendship"]));
    }

    [Fact]
    public void WhenIntentHasNoRuleThenConfigurationErrorIsThrownEvenIfAnotherPairMatches()
    {
        Assert.Throws<InvalidOperationException>(() => IntentCompatibilityService.AreCompatible(["friendship", "unknown"], ["friendship"]));
    }
}
