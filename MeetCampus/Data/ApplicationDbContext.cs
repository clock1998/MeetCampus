using MeetCampus.Data.Profiles;
using MeetCampus.Data.User;
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
    }
}
