using MeetCampus.Data;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeetCampus.IntegrationTests.Data;

public sealed class ApplicationDbContextMigrationTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task WhenMigratingAnEmptyPostgreSqlDatabaseThenIdentityAndProfileTablesAreCreated()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;

        await using var dbContext = new ApplicationDbContext(options);

        await dbContext.Database.MigrateAsync();

        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
        var userCount = await dbContext.Users.CountAsync();
        var profileCount = await dbContext.UserProfiles.CountAsync();
        var profileImageCount = await dbContext.UserProfileImages.CountAsync();

        Assert.NotEmpty(appliedMigrations);
        Assert.Equal(0, userCount);
        Assert.Equal(0, profileCount);
        Assert.Equal(0, profileImageCount);
    }
}
