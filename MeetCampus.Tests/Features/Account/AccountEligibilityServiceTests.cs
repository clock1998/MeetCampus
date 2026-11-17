using MeetCampus.Contracts.Features.Auth;
using MeetCampus.Data;
using MeetCampus.Data.Profiles;
using MeetCampus.Data.User;
using MeetCampus.Features.Account.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit;

namespace MeetCampus.Tests.Features.Account;

public sealed class AccountEligibilityServiceTests : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly AccountEligibilityService _service;

    public AccountEligibilityServiceTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>();
        services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<ApplicationDbContext>();
        services.AddScoped<AccountEligibilityService>();
        _services = services.BuildServiceProvider();
        _service = _services.GetRequiredService<AccountEligibilityService>();
    }

    public void Dispose() => _services.Dispose();

    [Fact]
    public async Task WhenUserIsNullThenEligibilityCheckThrows()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetEligibilityAsync(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void WhenEveryRequirementIsMetThenAccountIsEligibleForDiscoverability()
    {
        var eligibility = EligibleAccount();

        var result = eligibility.IsEligibleForDiscoverability;

        Assert.True(result);
    }

    [Fact]
    public void WhenMultipleFailuresAreSerializedThenTheyRoundTrip()
    {
        var eligibility = new AccountEligibilityResponse(AccountEligibilityFailure.AnswersIncomplete | AccountEligibilityFailure.EmailNotVerified | AccountEligibilityFailure.ModerationRestriction);
        var json = JsonSerializer.Serialize(eligibility, JsonSerializerOptions.Web);

        var result = JsonSerializer.Deserialize<AccountEligibilityResponse>(json, JsonSerializerOptions.Web);

        Assert.Equal(eligibility, result);
    }

    [Theory]
    [InlineData("email")]
    [InlineData("active")]
    [InlineData("profile")]
    [InlineData("image")]
    [InlineData("answers")]
    [InlineData("opt-in")]
    [InlineData("capacity")]
    [InlineData("moderation")]
    public void WhenAnyRequirementIsMissingThenAccountIsNotEligibleForDiscoverability(string requirement)
    {
        var eligibility = MissingRequirement(requirement);

        var result = eligibility.IsEligibleForDiscoverability;

        Assert.False(result);
    }

    [Fact]
    public void WhenProfileHasAllRequiredDetailsThenItIsComplete()
    {
        var profile = new UserProfile
        {
            StudyDomainId = Guid.NewGuid(), SchoolId = Guid.NewGuid(), LanguageId = Guid.NewGuid(),
            EthnicityId = Guid.NewGuid(), Intentions = [new UserProfileIntention()]
        };

        Assert.True(_service.IsProfileComplete(profile));
    }

    [Fact]
    public void WhenProfileIsNullThenItIsIncomplete()
    {
        Assert.False(_service.IsProfileComplete(null));
    }

    [Fact]
    public void WhenLifecycleIsNullThenAccountIsNotActive()
    {
        Assert.False(_service.IsAccountActive(null));
    }

    [Fact]
    public void WhenLifecycleIsNullThenDiscoverabilityIsNotOptedIn()
    {
        Assert.False(_service.IsDiscoverabilityOptedIn(null));
    }

    [Fact]
    public void WhenLifecycleIsActiveThenAccountIsActive()
    {
        Assert.True(_service.IsAccountActive(new AccountLifecycle { Status = AccountStatus.Active }));
    }

    [Fact]
    public void WhenLifecycleOptsInThenDiscoverabilityIsOptedIn()
    {
        Assert.True(_service.IsDiscoverabilityOptedIn(new AccountLifecycle { DiscoverabilityOptIn = true }));
    }

    private static AccountEligibilityResponse EligibleAccount() => new(AccountEligibilityFailure.None);

    private static AccountEligibilityResponse MissingRequirement(string requirement) => requirement switch
    {
        "email" => new(AccountEligibilityFailure.EmailNotVerified),
        "active" => new(AccountEligibilityFailure.AccountNotActive),
        "profile" => new(AccountEligibilityFailure.ProfileIncomplete),
        "image" => new(AccountEligibilityFailure.ProfileImageMissing),
        "answers" => new(AccountEligibilityFailure.AnswersIncomplete),
        "opt-in" => new(AccountEligibilityFailure.DiscoverabilityNotOptedIn),
        "capacity" => new(AccountEligibilityFailure.RecipientCapacityReached),
        "moderation" => new(AccountEligibilityFailure.ModerationRestriction),
        _ => throw new ArgumentOutOfRangeException(nameof(requirement))
    };
}
