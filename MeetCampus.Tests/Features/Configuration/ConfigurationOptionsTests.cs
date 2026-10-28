using System.ComponentModel.DataAnnotations;
using MeetCampus.Features.Configuration;
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
