using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        internal const string AdminRoleId = "10000000-0000-0000-0000-000000000001";
        internal const string PowerUserRoleId = "10000000-0000-0000-0000-000000000002";
        internal const string UserRoleId = "10000000-0000-0000-0000-000000000003";

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
                    Id = AdminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "6ed3e4b6-231f-41dd-9403-1595f6b8e73d"
                },
                new IdentityRole
                {
                    Id = PowerUserRoleId,
                    Name = "PowerUser",
                    NormalizedName = "POWERUSER",
                    ConcurrencyStamp = "8f3ec64a-af77-4f56-82f4-c9cea9eebd89"
                },
                new IdentityRole
                {
                    Id = UserRoleId,
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "b7d5672e-24dc-4c11-a948-6c89de8c3a40"
                }
            };

            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}
