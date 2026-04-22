using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            SeedRoles(builder);
        }

        private static void SeedRoles(ModelBuilder builder)
        {
            var roles = new[]
            {
                new IdentityRole
                {
                    Id = IdentityRoles.AdminRoleId,
                    Name = IdentityRoles.Admin,
                    NormalizedName = IdentityRoles.Admin.ToUpperInvariant(),
                    ConcurrencyStamp = "6ed3e4b6-231f-41dd-9403-1595f6b8e73d"
                },
                new IdentityRole
                {
                    Id = IdentityRoles.PowerUserRoleId,
                    Name = IdentityRoles.PowerUser,
                    NormalizedName = IdentityRoles.PowerUser.ToUpperInvariant(),
                    ConcurrencyStamp = "8f3ec64a-af77-4f56-82f4-c9cea9eebd89"
                },
                new IdentityRole
                {
                    Id = IdentityRoles.UserRoleId,
                    Name = IdentityRoles.User,
                    NormalizedName = IdentityRoles.User.ToUpperInvariant(),
                    ConcurrencyStamp = "b7d5672e-24dc-4c11-a948-6c89de8c3a40"
                }
            };

            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}
