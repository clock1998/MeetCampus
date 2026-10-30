using MeetCampus.Data;
using MeetCampus.Data.Profiles;
using MeetCampus.Data.User;
using MeetCampus.Features.Profile.Services;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace MeetCampus.IntegrationTests.Features.Profile;

public sealed class ProfileImageServiceTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task WhenImageContentIsNotSupportedThenUploadIsRejectedBeforeStorage()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var user = await CreateUserAsync(dbContext);
        await CreateProfileAsync(dbContext, user.Id);
        var imageStorage = new S3ProfileImageStorage(Options.Create(new S3Options()));
        var service = new ProfileImageService(dbContext, imageStorage, NullLogger<ProfileImageService>.Instance);
        using var content = new MemoryStream([0x00, 0x01, 0x02, 0x03]);
        var file = new FormFile(content, 0, content.Length, "files", "invalid.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        await Assert.ThrowsAsync<InvalidDataException>(() => service.UploadProfileImagesAsync(
            user.Id,
            [file],
            TestContext.Current.CancellationToken));
        Assert.Empty(await dbContext.UserProfileImages
            .Where(image => image.UserProfile.ApplicationUserId == user.Id)
            .ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WhenAnotherUserDeletesAnImageThenImageRemainsOwnedByOriginalUser()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var owner = await CreateUserAsync(dbContext);
        var otherUser = await CreateUserAsync(dbContext);
        var ownerProfile = await CreateProfileAsync(dbContext, owner.Id);
        await CreateProfileAsync(dbContext, otherUser.Id);
        var image = new UserProfileImage
        {
            Id = Guid.NewGuid(),
            UserProfileId = ownerProfile.Id,
            ImageKey = $"profile-images/{ownerProfile.Id}/{Guid.NewGuid():N}.webp",
            IsAvatar = true
        };
        dbContext.UserProfileImages.Add(image);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var imageStorage = new S3ProfileImageStorage(Options.Create(new S3Options()));
        var service = new ProfileImageService(dbContext, imageStorage, NullLogger<ProfileImageService>.Instance);

        var wasDeleted = await service.DeleteProfileImageAsync(
            otherUser.Id,
            image.Id,
            TestContext.Current.CancellationToken);

        Assert.False(wasDeleted);
        Assert.True(await dbContext.UserProfileImages.AnyAsync(
            profileImage => profileImage.Id == image.Id,
            TestContext.Current.CancellationToken));
    }

    private ApplicationDbContext CreateDbContext() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(fixture.ConnectionString)
        .Options);

    private static async Task<ApplicationUser> CreateUserAsync(ApplicationDbContext dbContext)
    {
        var email = $"profile-image-{Guid.NewGuid():N}@example.edu";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return user;
    }

    private static async Task<UserProfile> CreateProfileAsync(ApplicationDbContext dbContext, Guid userId)
    {
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = userId,
            StudyDomainId = await dbContext.StudyDomains.Select(item => item.Id).FirstAsync(TestContext.Current.CancellationToken),
            SchoolId = await dbContext.Schools.Select(item => item.Id).FirstAsync(TestContext.Current.CancellationToken),
            LanguageId = await dbContext.CampusLanguages.Select(item => item.Id).FirstAsync(TestContext.Current.CancellationToken),
            EthnicityId = await dbContext.UserEthnicities.Select(item => item.Id).FirstAsync(TestContext.Current.CancellationToken)
        };
        dbContext.UserProfiles.Add(profile);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return profile;
    }
}
