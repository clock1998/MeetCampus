using MeetCampus.Data;
using MeetCampus.Data.User;
using MeetCampus.Features.Auth.Infrastructure;
using MeetCampus.Features.Auth.Services;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeetCampus.IntegrationTests.Features.Auth;

public sealed class InitialAdministratorProvisionerTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task WhenProvisioningNewAdministratorThenUserIsCreatedConfirmedAndAssignedAdminRole()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var provisioner = scope.ServiceProvider.GetRequiredService<InitialAdministratorProvisioner>();
        var email = $"admin-{Guid.NewGuid():N}@example.test";
        const string password = "Strong!Password-1";

        await provisioner.ProvisionAsync(email, password, cancellationToken);

        var administrator = await userManager.FindByEmailAsync(email);
        Assert.NotNull(administrator);
        Assert.True(administrator.EmailConfirmed);
        Assert.Contains("Administrator", await userManager.GetRolesAsync(administrator));

        await provisioner.ProvisionAsync(email, "Different!Password-2", cancellationToken);

        Assert.True(await userManager.CheckPasswordAsync(administrator, password));
    }

    [Fact]
    public async Task WhenEmailBelongsToNonAdministratorThenProvisioningDoesNotElevateTheAccount()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var provisioner = scope.ServiceProvider.GetRequiredService<InitialAdministratorProvisioner>();
        var email = $"student-{Guid.NewGuid():N}@example.test";
        var existingUser = new ApplicationUser { UserName = email, Email = email };
        var createResult = await userManager.CreateAsync(existingUser, "Strong!Password-1");
        Assert.True(createResult.Succeeded);

        await Assert.ThrowsAsync<InvalidOperationException>(() => provisioner.ProvisionAsync(email, "Strong!Password-2", cancellationToken));

        Assert.DoesNotContain("Administrator", await userManager.GetRolesAsync(existingUser));
    }

    private ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(fixture.ConnectionString));
        services.AddMeetCampusIdentityCore();
        services.AddScoped<InitialAdministratorProvisioner>();

        return services.BuildServiceProvider();
    }
}
