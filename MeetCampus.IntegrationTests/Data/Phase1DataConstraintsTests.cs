using MeetCampus.Data;
using MeetCampus.Data.Matching;
using MeetCampus.Data.Safety;
using MeetCampus.Data.User;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeetCampus.IntegrationTests.Data;

public sealed class Phase1DataConstraintsTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task WhenEmailDomainIsDuplicatedThenUniqueConstraintRejectsIt()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var domain = $"{Guid.NewGuid():N}.example.edu";
        dbContext.AllowedEmailDomains.AddRange(
            new AllowedEmailDomain { Id = Guid.NewGuid(), Domain = domain },
            new AllowedEmailDomain { Id = Guid.NewGuid(), Domain = domain });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WhenIntentRuleHasAnUnknownIntentThenForeignKeyRejectsIt()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var intentId = Guid.Parse("30000000-0000-0000-0000-000000000001");
        dbContext.IntentCompatibilityRules.Add(new IntentCompatibilityRule
        {
            Id = Guid.NewGuid(),
            SenderIntentId = Guid.NewGuid(),
            RecipientIntentId = intentId
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WhenBlockReferencesAnUnknownUserThenForeignKeyRejectsIt()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var blocker = await CreateUserAsync(dbContext, "blocker");
        dbContext.UserBlocks.Add(new UserBlock
        {
            Id = Guid.NewGuid(),
            BlockerUserId = blocker.Id,
            BlockedUserId = Guid.NewGuid()
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WhenCreatingDuplicateDirectionalBlocksThenUniqueConstraintRejectsSecondBlock()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var blocker = await CreateUserAsync(dbContext, "blocker-unique");
        var blocked = await CreateUserAsync(dbContext, "blocked-unique");
        dbContext.UserBlocks.Add(new UserBlock { Id = Guid.NewGuid(), BlockerUserId = blocker.Id, BlockedUserId = blocked.Id });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.UserBlocks.Add(new UserBlock { Id = Guid.NewGuid(), BlockerUserId = blocker.Id, BlockedUserId = blocked.Id });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    private ApplicationDbContext CreateDbContext() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(fixture.ConnectionString)
        .Options);

    private static async Task<ApplicationUser> CreateUserAsync(ApplicationDbContext dbContext, string name)
    {
        var email = $"{name}-{Guid.NewGuid():N}@example.edu";
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
}
