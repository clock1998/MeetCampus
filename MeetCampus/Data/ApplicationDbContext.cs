using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<StudyDomain> StudyDomains => Set<StudyDomain>();
        public DbSet<School> Schools => Set<School>();
        public DbSet<Language> CampusLanguages => Set<Language>();
        public DbSet<UserIntention> UserIntentions => Set<UserIntention>();
        public DbSet<UserEthnicity> UserEthnicities => Set<UserEthnicity>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            SeedRoles(builder);
            ConfigureUserProfile(builder);
            SeedProfileLookups(builder);
        }

        private static void ConfigureUserProfile(ModelBuilder builder)
        {
            builder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(profile => profile.Id);

                entity.Property(profile => profile.Gender)
                    .HasMaxLength(64);

                entity.Property(profile => profile.Sexuality)
                    .HasMaxLength(64);

                entity.HasOne(profile => profile.ApplicationUser)
                    .WithOne(user => user.UserProfile)
                    .HasForeignKey<UserProfile>(profile => profile.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(profile => profile.StudyDomain)
                    .WithMany()
                    .HasForeignKey(profile => profile.StudyDomainId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(profile => profile.School)
                    .WithMany()
                    .HasForeignKey(profile => profile.SchoolId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(profile => profile.Language)
                    .WithMany()
                    .HasForeignKey(profile => profile.LanguageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(profile => profile.Intention)
                    .WithMany()
                    .HasForeignKey(profile => profile.IntentionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(profile => profile.Ethnicity)
                    .WithMany()
                    .HasForeignKey(profile => profile.EthnicityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<StudyDomain>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Name).HasMaxLength(128);
            });

            builder.Entity<School>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Name).HasMaxLength(128);
            });

            builder.Entity<Language>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Name).HasMaxLength(128);
            });

            builder.Entity<UserIntention>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Code).HasMaxLength(64);
                entity.Property(item => item.Name).HasMaxLength(128);
                entity.Property(item => item.DisplayKey).HasMaxLength(128);
                entity.HasIndex(item => item.Code).IsUnique();
            });

            builder.Entity<UserEthnicity>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Code).HasMaxLength(64);
                entity.Property(item => item.Name).HasMaxLength(128);
                entity.Property(item => item.DisplayKey).HasMaxLength(128);
                entity.HasIndex(item => item.Code).IsUnique();
            });
        }

        private static void SeedProfileLookups(ModelBuilder builder)
        {
            builder.Entity<UserIntention>().HasData(
                new UserIntention
                {
                    Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                    Code = "friendship",
                    Name = "Friendship",
                    DisplayKey = "Profile_Intention_Friendship"
                },
                new UserIntention
                {
                    Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                    Code = "dating",
                    Name = "Dating",
                    DisplayKey = "Profile_Intention_Dating"
                },
                new UserIntention
                {
                    Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                    Code = "relationship",
                    Name = "Relationship",
                    DisplayKey = "Profile_Intention_Relationship"
                },
                new UserIntention
                {
                    Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
                    Code = "networking",
                    Name = "Networking",
                    DisplayKey = "Profile_Intention_Networking"
                });

            builder.Entity<UserEthnicity>().HasData(
                new UserEthnicity
                {
                    Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                    Code = "prefer-not-to-say",
                    Name = "Prefer not to say",
                    DisplayKey = "Profile_Ethnicity_PreferNotToSay"
                },
                new UserEthnicity
                {
                    Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                    Code = "arab",
                    Name = "Arab",
                    DisplayKey = "Profile_Ethnicity_Arab"
                },
                new UserEthnicity
                {
                    Id = Guid.Parse("40000000-0000-0000-0000-000000000003"),
                    Code = "black",
                    Name = "Black",
                    DisplayKey = "Profile_Ethnicity_Black"
                },
                new UserEthnicity
                {
                    Id = Guid.Parse("40000000-0000-0000-0000-000000000004"),
                    Code = "east-asian",
                    Name = "East Asian",
                    DisplayKey = "Profile_Ethnicity_EastAsian"
                },
                new UserEthnicity
                {
                    Id = Guid.Parse("40000000-0000-0000-0000-000000000005"),
                    Code = "indigenous",
                    Name = "Indigenous",
                    DisplayKey = "Profile_Ethnicity_Indigenous"
                },
                new UserEthnicity
                {
                    Id = Guid.Parse("40000000-0000-0000-0000-000000000006"),
                    Code = "latino",
                    Name = "Latino",
                    DisplayKey = "Profile_Ethnicity_Latino"
                },
                new UserEthnicity
                {
                    Id = Guid.Parse("40000000-0000-0000-0000-000000000007"),
                    Code = "middle-eastern",
                    Name = "Middle Eastern",
                    DisplayKey = "Profile_Ethnicity_MiddleEastern"
                },
                new UserEthnicity
                {
                    Id = Guid.Parse("40000000-0000-0000-0000-000000000008"),
                    Code = "south-asian",
                    Name = "South Asian",
                    DisplayKey = "Profile_Ethnicity_SouthAsian"
                },
                new UserEthnicity
                {
                    Id = Guid.Parse("40000000-0000-0000-0000-000000000009"),
                    Code = "southeast-asian",
                    Name = "Southeast Asian",
                    DisplayKey = "Profile_Ethnicity_SoutheastAsian"
                },
                new UserEthnicity
                {
                    Id = Guid.Parse("40000000-0000-0000-0000-000000000010"),
                    Code = "white",
                    Name = "White",
                    DisplayKey = "Profile_Ethnicity_White"
                },
                new UserEthnicity
                {
                    Id = Guid.Parse("40000000-0000-0000-0000-000000000011"),
                    Code = "mixed",
                    Name = "Mixed",
                    DisplayKey = "Profile_Ethnicity_Mixed"
                },
                new UserEthnicity
                {
                    Id = Guid.Parse("40000000-0000-0000-0000-000000000012"),
                    Code = "other",
                    Name = "Other",
                    DisplayKey = "Profile_Ethnicity_Other"
                });
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
