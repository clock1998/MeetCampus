using MeetCampus.Configuration;
using MeetCampus.Data;
using MeetCampus.Data.Profiles;
using MeetCampus.Data.Safety;
using MeetCampus.Data.User;
using MeetCampus.Features.Account.Services;
using MeetCampus.Features.Matching.Services;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace MeetCampus.IntegrationTests.Features.Matching;

public sealed class MatchEligibilityServiceTests(PostgreSqlContainerFixture fixture) : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task WhenEligibleMembersShareAnIntentThenTheyCanMatch()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var first = await CreateEligibleUserAsync(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        var second = await CreateEligibleUserAsync(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        var service = CreateMatchService(scope.ServiceProvider);

        var result = await service.CanMatchAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenEitherMemberBlocksTheOtherThenTheyCannotMatch(bool firstBlocksSecond)
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var first = await CreateEligibleUserAsync(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        var second = await CreateEligibleUserAsync(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        db.UserBlocks.Add(new UserBlock
        {
            Id = Guid.NewGuid(), BlockerUserId = firstBlocksSecond ? first.Id : second.Id,
            BlockedUserId = firstBlocksSecond ? second.Id : first.Id, CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateMatchService(scope.ServiceProvider).CanMatchAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    [Fact]
    public async Task WhenMemberOptsOutThenPairCannotMatch()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var first = await CreateEligibleUserAsync(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        var second = await CreateEligibleUserAsync(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        var lifecycle = await db.AccountLifecycles.SingleAsync(item => item.ApplicationUserId == second.Id, TestContext.Current.CancellationToken);
        lifecycle.DiscoverabilityOptIn = false;
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateMatchService(scope.ServiceProvider).CanMatchAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    [Fact]
    public async Task WhenMembersHaveDifferentIntentsThenTheyCannotMatch()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var first = await CreateEligibleUserAsync(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        var second = await CreateEligibleUserAsync(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), "30000000-0000-0000-0000-000000000003");

        var result = await CreateMatchService(scope.ServiceProvider).CanMatchAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    [Fact]
    public async Task WhenMemberIsComparedWithThemselvesThenTheyCannotMatch()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var id = Guid.NewGuid();

        var result = await CreateMatchService(scope.ServiceProvider).CanMatchAsync(id, id, TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    private ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(fixture.ConnectionString));
        services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<ApplicationDbContext>();
        return services.BuildServiceProvider();
    }

    private static MatchEligibilityService CreateMatchService(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        return new MatchEligibilityService(db, new AccountEligibilityService(db, services.GetRequiredService<UserManager<ApplicationUser>>(), Options.Create(new ContactOptions())));
    }

    private static async Task<ApplicationUser> CreateEligibleUserAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager, string intentId = "30000000-0000-0000-0000-000000000001")
    {
        var email = $"matching-{Guid.NewGuid():N}@example.edu";
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        Assert.True((await userManager.CreateAsync(user, "Test-password1!")).Succeeded);
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(), ApplicationUserId = user.Id,
            StudyDomainId = Guid.Parse("10000000-0000-0000-0000-000000000011"),
            SchoolId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            LanguageId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
            EthnicityId = Guid.Parse("40000000-0000-0000-0000-000000000001")
        };
        db.UserProfiles.Add(profile);
        db.UserProfileIntentions.Add(new UserProfileIntention { UserProfileId = profile.Id, UserIntentionId = Guid.Parse(intentId) });
        db.UserProfileImages.Add(new UserProfileImage { Id = Guid.NewGuid(), UserProfileId = profile.Id, ImageKey = $"match/{user.Id:N}.webp", IsAvatar = true });
        db.AccountLifecycles.Add(new AccountLifecycle { ApplicationUserId = user.Id, Status = AccountStatus.Active, DiscoverabilityOptIn = true });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return user;
    }
}
