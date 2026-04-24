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
        public DbSet<Gender> Genders => Set<Gender>();
        public DbSet<Sexuality> Sexualities => Set<Sexuality>();

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

                entity.HasOne(profile => profile.Gender)
                    .WithMany()
                    .HasForeignKey(profile => profile.GenderId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasOne(profile => profile.Sexuality)
                    .WithMany()
                    .HasForeignKey(profile => profile.SexualityId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

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

            builder.Entity<Gender>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Code).HasMaxLength(64);
                entity.Property(item => item.Name).HasMaxLength(128);
                entity.Property(item => item.DisplayKey).HasMaxLength(128);
                entity.HasIndex(item => item.Code).IsUnique();
            });

            builder.Entity<Sexuality>(entity =>
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
            builder.Entity<StudyDomain>().HasData(
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "Accounting" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "Aerospace Engineering" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "Architecture" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Name = "Art & Design" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Name = "Biology" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000006"), Name = "Business Administration" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000007"), Name = "Chemical Engineering" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000008"), Name = "Chemistry" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000009"), Name = "Civil Engineering" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000010"), Name = "Communications" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000011"), Name = "Computer Science" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000012"), Name = "Data Science" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000013"), Name = "Economics" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000014"), Name = "Education" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000015"), Name = "Electrical Engineering" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000016"), Name = "Environmental Science" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000017"), Name = "Film & Media Studies" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000018"), Name = "Finance" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000019"), Name = "Fine Arts" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000020"), Name = "Geography" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000021"), Name = "History" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000022"), Name = "Information Systems" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000023"), Name = "Journalism" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000024"), Name = "Kinesiology" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000025"), Name = "Law" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000026"), Name = "Linguistics" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000027"), Name = "Marketing" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000028"), Name = "Mathematics" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000029"), Name = "Mechanical Engineering" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000030"), Name = "Medicine" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000031"), Name = "Music" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000032"), Name = "Nursing" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000033"), Name = "Nutrition & Dietetics" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000034"), Name = "Pharmacy" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000035"), Name = "Philosophy" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000036"), Name = "Physics" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000037"), Name = "Political Science" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000038"), Name = "Psychology" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000039"), Name = "Public Health" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000040"), Name = "Social Work" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000041"), Name = "Sociology" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000042"), Name = "Software Engineering" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000043"), Name = "Statistics" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000044"), Name = "Theatre" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000045"), Name = "Urban Planning" },
                new StudyDomain { Id = Guid.Parse("10000000-0000-0000-0000-000000000046"), Name = "Other" });

            builder.Entity<School>().HasData(
                new School { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), Name = "Concordia University" },
                new School { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), Name = "McGill University" },
                new School { Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), Name = "Université du Québec à Montréal (UQAM)" },
                new School { Id = Guid.Parse("20000000-0000-0000-0000-000000000004"), Name = "Université de Montréal (UdeM)" });

            builder.Entity<Language>().HasData(
                new Language { Id = Guid.Parse("70000000-0000-0000-0000-000000000001"), Name = "English" },
                new Language { Id = Guid.Parse("70000000-0000-0000-0000-000000000002"), Name = "French" },
                new Language { Id = Guid.Parse("70000000-0000-0000-0000-000000000003"), Name = "Mandarin Chinese" },
                new Language { Id = Guid.Parse("70000000-0000-0000-0000-000000000004"), Name = "Hindi" },
                new Language { Id = Guid.Parse("70000000-0000-0000-0000-000000000005"), Name = "Spanish" },
                new Language { Id = Guid.Parse("70000000-0000-0000-0000-000000000006"), Name = "Arabic" },
                new Language { Id = Guid.Parse("70000000-0000-0000-0000-000000000007"), Name = "Bengali" },
                new Language { Id = Guid.Parse("70000000-0000-0000-0000-000000000008"), Name = "Portuguese" },
                new Language { Id = Guid.Parse("70000000-0000-0000-0000-000000000009"), Name = "Russian" },
                new Language { Id = Guid.Parse("70000000-0000-0000-0000-000000000010"), Name = "Urdu" });
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

            builder.Entity<Gender>().HasData(
                new Gender { Id = Guid.Parse("50000000-0000-0000-0000-000000000001"), Code = "man", Name = "Man", DisplayKey = "Profile_Gender_Man" },
                new Gender { Id = Guid.Parse("50000000-0000-0000-0000-000000000002"), Code = "woman", Name = "Woman", DisplayKey = "Profile_Gender_Woman" },
                new Gender { Id = Guid.Parse("50000000-0000-0000-0000-000000000003"), Code = "other", Name = "Other", DisplayKey = "Profile_Gender_Other" });

            builder.Entity<Sexuality>().HasData(
                new Sexuality { Id = Guid.Parse("60000000-0000-0000-0000-000000000001"), Code = "straight", Name = "Straight", DisplayKey = "Profile_Sexuality_Straight" },
                new Sexuality { Id = Guid.Parse("60000000-0000-0000-0000-000000000002"), Code = "gay", Name = "Gay", DisplayKey = "Profile_Sexuality_Gay" },
                new Sexuality { Id = Guid.Parse("60000000-0000-0000-0000-000000000003"), Code = "lesbian", Name = "Lesbian", DisplayKey = "Profile_Sexuality_Lesbian" },
                new Sexuality { Id = Guid.Parse("60000000-0000-0000-0000-000000000004"), Code = "bisexual", Name = "Bisexual", DisplayKey = "Profile_Sexuality_Bisexual" },
                new Sexuality { Id = Guid.Parse("60000000-0000-0000-0000-000000000005"), Code = "pansexual", Name = "Pansexual", DisplayKey = "Profile_Sexuality_Pansexual" },
                new Sexuality { Id = Guid.Parse("60000000-0000-0000-0000-000000000006"), Code = "asexual", Name = "Asexual", DisplayKey = "Profile_Sexuality_Asexual" },
                new Sexuality { Id = Guid.Parse("60000000-0000-0000-0000-000000000007"), Code = "queer", Name = "Queer", DisplayKey = "Profile_Sexuality_Queer" },
                new Sexuality { Id = Guid.Parse("60000000-0000-0000-0000-000000000008"), Code = "prefer-not-to-say", Name = "Prefer not to say", DisplayKey = "Profile_Sexuality_PreferNotToSay" });
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
