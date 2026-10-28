using MeetCampus.Data;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeetCampus.IntegrationTests.Data;

public sealed class ApplicationDbContextMigrationTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task Database_migrations_apply_successfully()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);

        Assert.False(dbContext.Database.HasPendingModelChanges());
    }
}
