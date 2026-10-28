using System.Security.Claims;
using MeetCampus.Data;
using MeetCampus.Data.User;
using MeetCampus.Features.Administration.Services;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeetCampus.IntegrationTests.Features.Administration;

public sealed class AdministrativeDataServiceTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task WhenAdministratorChangesEmailDomainThenChangeAndAuditArePersisted()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var administrator = await CreateUserAsync(dbContext);
        var principal = CreatePrincipal(administrator.Id.ToString(), "Administrator");
        var service = scope.ServiceProvider.GetRequiredService<AdministrativeDataService>();
        var domain = $"{Guid.NewGuid():N}.example.edu";

        await service.SetAllowedEmailDomainAsync(principal, domain, isEnabled: true, TestContext.Current.CancellationToken);

        Assert.True(await dbContext.AllowedEmailDomains.AnyAsync(item => item.Domain == domain, TestContext.Current.CancellationToken));
        Assert.True(await dbContext.AuditEvents.AnyAsync(item => item.ActorUserId == administrator.Id && item.Action == "EmailDomain.SetEnabled", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WhenNonAdministratorChangesEmailDomainThenOperationIsRejected()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<AdministrativeDataService>();
        var principal = CreatePrincipal(Guid.NewGuid().ToString(), "Student");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.SetAllowedEmailDomainAsync(
            principal,
            $"{Guid.NewGuid():N}.example.edu",
            isEnabled: true,
            TestContext.Current.CancellationToken));
    }

    private ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options => options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator")));
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(fixture.ConnectionString));
        services.AddScoped<AdministrativeDataService>();
        return services.BuildServiceProvider();
    }

    private static ClaimsPrincipal CreatePrincipal(string userId, string role) => new(
        new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        ],
        authenticationType: "integration-test"));

    private static async Task<ApplicationUser> CreateUserAsync(ApplicationDbContext dbContext)
    {
        var email = $"admin-{Guid.NewGuid():N}@example.edu";
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
