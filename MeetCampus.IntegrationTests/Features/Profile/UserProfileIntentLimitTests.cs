using MeetCampus.Contracts.Features.Profile;
using MeetCampus.Data;
using MeetCampus.Data.User;
using MeetCampus.Features.Profile.Services;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace MeetCampus.IntegrationTests.Features.Profile;

public sealed class UserProfileIntentLimitTests(PostgreSqlContainerFixture fixture) : IClassFixture<PostgreSqlContainerFixture>
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task WhenOneOrTwoIntentionsAreSelectedThenProfileIsSaved(int count)
    {
        await using var db = await CreateContextAsync();
        var user = AddUser(db);
        var request = CreateRequest(count);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        using var storage = new S3ProfileImageStorage(Options.Create(new S3Options()));
        var service = new UserProfileService(db, new ProfileImageService(db, storage, NullLogger<ProfileImageService>.Instance));

        await service.UpdateProfileAsync(user.Id, request, TestContext.Current.CancellationToken);

        Assert.Equal(count, await db.UserProfileIntentions.CountAsync(item => item.UserProfile.ApplicationUserId == user.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WhenThreeIntentionsAreSelectedThenProfileIsNotSaved()
    {
        await using var db = await CreateContextAsync();
        var user = AddUser(db);
        var request = CreateRequest(3);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        using var storage = new S3ProfileImageStorage(Options.Create(new S3Options()));
        var service = new UserProfileService(db, new ProfileImageService(db, storage, NullLogger<ProfileImageService>.Instance));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateProfileAsync(user.Id, request, TestContext.Current.CancellationToken));

        Assert.False(await db.UserProfiles.AnyAsync(profile => profile.ApplicationUserId == user.Id, TestContext.Current.CancellationToken));
    }

    private async Task<ApplicationDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(fixture.ConnectionString).Options;
        var db = new ApplicationDbContext(options);
        await db.Database.MigrateAsync(TestContext.Current.CancellationToken);
        return db;
    }

    private static ApplicationUser AddUser(ApplicationDbContext db)
    {
        var id = Guid.NewGuid();
        var user = new ApplicationUser { Id = id, UserName = $"intent-{id:N}@example.edu", SecurityStamp = Guid.NewGuid().ToString() };
        db.Users.Add(user);
        return user;
    }

    private static UpdateUserProfileRequest CreateRequest(int count) => new()
    {
        StudyDomainId = Guid.Parse("10000000-0000-0000-0000-000000000011"),
        SchoolId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
        LanguageId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
        EthnicityId = Guid.Parse("40000000-0000-0000-0000-000000000001"),
        IntentionIds = Enumerable.Range(1, count).Select(index => Guid.Parse($"30000000-0000-0000-0000-{index:000000000000}")).ToList()
    };
}
