using MeetCampus.Data.Profiles;
using MeetCampus.Data.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Data;

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
        public DbSet<UserProfileImage> UserProfileImages => Set<UserProfileImage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        SeedHelper.SeedRoles(builder);
        ApplicationDbContextHelpers.ConfigureEntities(builder);
        SeedHelper.SeedProfileLookups(builder);

        builder.Entity<IdentityUserPasskey<string>>(entity =>
        {
            entity.ToTable("AspNetUserPasskeys");
            entity.HasKey(passkey => passkey.CredentialId);
            entity.Property(passkey => passkey.CredentialId).HasMaxLength(1024);
            entity.Property(passkey => passkey.UserId).IsRequired();
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(passkey => passkey.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.OwnsOne(passkey => passkey.Data, owned =>
                owned.ToJson("Data").HasColumnType("jsonb"));
        });

        builder.Entity<ApplicationUser>()
            .Property(user => user.PhoneNumber)
            .HasMaxLength(256);

        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.Property(login => login.LoginProvider).HasMaxLength(128);
            entity.Property(login => login.ProviderKey).HasMaxLength(128);
        });

        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.Property(token => token.LoginProvider).HasMaxLength(128);
            entity.Property(token => token.Name).HasMaxLength(128);
        });
    }
}
