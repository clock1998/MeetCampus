using MeetCampus.Data;
using Microsoft.AspNetCore.Identity;

namespace MeetCampus
{
    public static class Seed
    {
        public static async Task SeedIdentityDataAsync(
        IConfiguration configuration,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
            {
                ArgumentNullException.ThrowIfNull(configuration);
                ArgumentNullException.ThrowIfNull(roleManager);
                ArgumentNullException.ThrowIfNull(userManager);

                var requiredRoles = new[] { IdentityRoles.Admin, IdentityRoles.PowerUser, IdentityRoles.User };
                foreach (var roleName in requiredRoles)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var createRoleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                        if (!createRoleResult.Succeeded)
                        {
                            throw new InvalidOperationException($"Failed to seed role '{roleName}': {string.Join(", ", createRoleResult.Errors.Select(e => e.Description))}");
                        }
                    }
                }

                var adminEmail = configuration["Seed:AdminUser:Email"];
                var adminUserName = configuration["Seed:AdminUser:UserName"];
                var adminPassword = configuration["Seed:AdminUser:Password"];

                if (string.IsNullOrWhiteSpace(adminEmail))
                {
                    throw new InvalidOperationException("Seed admin email is missing. Configure 'Seed:AdminUser:Email'.");
                }

                if (string.IsNullOrWhiteSpace(adminUserName))
                {
                    throw new InvalidOperationException("Seed admin user name is missing. Configure 'Seed:AdminUser:UserName'.");
                }

                if (string.IsNullOrWhiteSpace(adminPassword))
                {
                    throw new InvalidOperationException("Seed admin password is missing. Configure 'Seed:AdminUser:Password'.");
                }

                const string adminUserId = "20000000-0000-0000-0000-000000000001";

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser is null)
                {
                    adminUser = new ApplicationUser
                    {
                        Id = adminUserId,
                        UserName = adminUserName,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);
                    if (!createUserResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to seed admin user: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                    }
                }

                if (!await userManager.IsInRoleAsync(adminUser, IdentityRoles.Admin))
                {
                    var addRoleResult = await userManager.AddToRoleAsync(adminUser, IdentityRoles.Admin);
                    if (!addRoleResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to add admin user to role '{IdentityRoles.Admin}': {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }
}
