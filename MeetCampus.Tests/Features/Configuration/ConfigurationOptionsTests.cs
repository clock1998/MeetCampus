using MeetCampus.Configuration;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MeetCampus.Tests.Features.Configuration;

public sealed class ConfigurationOptionsTests
{
    [Fact]
    public void WhenMvpDefaultsAreUsedThenAllOptionsValidate()
    {
        AssertValid(new MatchingOptions());
        AssertValid(new ContactOptions());
        AssertValid(new PrivacyOptions());
        AssertValid(new SafetyOptions());
        AssertValid(new VerificationOptions());
    }

    [Fact]
    public void WhenMatchingWeightsDoNotSumToOneThenValidationFails()
    {
        var options = new MatchingOptions
        {
            Weights = new MatchingWeightsOptions { Interests = 0.2m }
        };

        AssertInvalid(options);
    }

    [Theory]
    [InlineData("interests", 0.25)]
    [InlineData("values", 0.30)]
    [InlineData("social", 0.20)]
    [InlineData("lifestyle", 0.15)]
    [InlineData("connection-intent", 0.10)]
    public void WhenCategoryIsSupportedThenConfiguredWeightIsReturned(string category, decimal expected)
    {
        var weights = new MatchingWeightsOptions();

        var weight = weights.GetCategoryWeight(category);

        Assert.Equal(expected, weight);
    }

    [Fact]
    public void WhenCategoryIsUnknownThenItIsRejected()
    {
        var weights = new MatchingWeightsOptions();

        Assert.Throws<ArgumentException>(() => weights.GetCategoryWeight("unknown"));
    }

    [Fact]
    public void WhenCategoryWeightIsConfiguredThenLookupUsesConfiguredValue()
    {
        var weights = new MatchingWeightsOptions { Interests = 0.30m, Values = 0.25m };

        var weight = weights.GetCategoryWeight("interests");

        Assert.Equal(0.30m, weight);
    }

    [Fact]
    public void WhenMatchingWeightsAreInvalidThenCategoryLookupIsRejected()
    {
        var weights = new MatchingWeightsOptions { Interests = 0.2m };

        Assert.Throws<InvalidOperationException>(() => weights.GetCategoryWeight("interests"));
    }

    [Fact]
    public void WhenMatchingLimitsAreContradictoryThenValidationFails()
    {
        var options = new MatchingOptions { NewRecommendationsPerUserPerDay = 10, MaxUnseenRecommendations = 9 };

        AssertInvalid(options);
    }

    [Fact]
    public void WhenContactQuotaIsNotPositiveThenValidationFails()
    {
        AssertInvalid(new ContactOptions { MaxPendingOutbound = 0 });
    }

    [Fact]
    public void WhenVerificationTokenLifetimeIsOutsideAllowedRangeThenValidationFails()
    {
        AssertInvalid(new VerificationOptions { TokenLifetimeMinutes = 2 });
    }

    [Fact]
    public void WhenMvpPrivacyInvariantIsDisabledThenValidationFails()
    {
        AssertInvalid(new PrivacyOptions { UniversityEmailPublic = true });
    }

    private static void AssertValid(object value)
    {
        Assert.True(Validator.TryValidateObject(value, new ValidationContext(value), [], validateAllProperties: true));
    }

    private static void AssertInvalid(object value)
    {
        Assert.False(Validator.TryValidateObject(value, new ValidationContext(value), [], validateAllProperties: true));
    }
}
