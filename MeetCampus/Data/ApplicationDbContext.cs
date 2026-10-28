using MeetCampus.Data.Profiles;
using MeetCampus.Data.Audit;
using MeetCampus.Data.Contact;
using MeetCampus.Data.Matching;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.Safety;
using MeetCampus.Data.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
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
        public DbSet<AccountLifecycle> AccountLifecycles => Set<AccountLifecycle>();
        public DbSet<AllowedEmailDomain> AllowedEmailDomains => Set<AllowedEmailDomain>();
        public DbSet<IntentCompatibilityRule> IntentCompatibilityRules => Set<IntentCompatibilityRule>();
        public DbSet<Question> QuestionDefinitions => Set<Question>();
        public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
        public DbSet<UserAnswer> QuestionnaireAnswerSets => Set<UserAnswer>();
        public DbSet<UserProfileIntention> UserProfileIntentions => Set<UserProfileIntention>();
        public DbSet<RecommendationBatch> RecommendationBatches => Set<RecommendationBatch>();
        public DbSet<RecommendationItem> RecommendationItems => Set<RecommendationItem>();
        public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
        public DbSet<UserBlock> UserBlocks => Set<UserBlock>();
        public DbSet<UserReport> UserReports => Set<UserReport>();
        public DbSet<ModerationAction> ModerationActions => Set<ModerationAction>();
        public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        SeedHelper.SeedRoles(builder);
        ApplicationDbContextHelpers.ConfigureEntities(builder);
        SeedHelper.SeedProfileLookups(builder);

        builder.Entity<IdentityUserPasskey<Guid>>(entity =>
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

        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.Property(login => login.LoginProvider).HasMaxLength(128);
            entity.Property(login => login.ProviderKey).HasMaxLength(128);
        });

        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.Property(token => token.LoginProvider).HasMaxLength(128);
            entity.Property(token => token.Name).HasMaxLength(128);
        });
    }

}
