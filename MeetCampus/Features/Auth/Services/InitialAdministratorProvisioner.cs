using MeetCampus.Data;
using MeetCampus.Data.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Features.Auth.Services;

public sealed class InitialAdministratorProvisioner(
    ApplicationDbContext dbContext,
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager)
{
    /// <summary>
    /// Creates the initial confirmed administrator using credentials provided by an explicitly invoked bootstrap command.
    /// </summary>
    public async Task ProvisionAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        if (!await roleManager.RoleExistsAsync(IdentityRoles.Admin))
            throw new InvalidOperationException("The Admin role is missing. Apply database migrations before bootstrapping an administrator.");

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, IdentityRoles.Admin))
                throw new InvalidOperationException("An account already exists for the configured administrator email and is not an administrator. Refusing to elevate it.");

            await transaction.CommitAsync(cancellationToken);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var administrator = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };
        var createResult = await userManager.CreateAsync(administrator, password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create the initial administrator: {string.Join(", ", createResult.Errors.Select(error => error.Description))}");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var addRoleResult = await userManager.AddToRoleAsync(administrator, IdentityRoles.Admin);
        if (!addRoleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to assign the Admin role: {string.Join(", ", addRoleResult.Errors.Select(error => error.Description))}");
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
