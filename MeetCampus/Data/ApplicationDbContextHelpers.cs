using MeetCampus.Data.Profiles;
using MeetCampus.Data.Audit;
using MeetCampus.Data.Contact;
using MeetCampus.Data.Matching;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.Safety;
using MeetCampus.Data.User;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Data
{
    internal static class ApplicationDbContextHelpers
    {
        internal static void ConfigureEntities(ModelBuilder builder)
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

            builder.Entity<UserProfileImage>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.ImageKey).HasMaxLength(256);
                entity.HasOne(image => image.UserProfile)
                    .WithMany(profile => profile.ProfileImages)
                    .HasForeignKey(image => image.UserProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AccountLifecycle>(entity =>
            {
                entity.HasKey(item => item.ApplicationUserId);
                entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
                entity.HasOne(item => item.ApplicationUser)
                    .WithOne(user => user.Lifecycle)
                    .HasForeignKey<AccountLifecycle>(item => item.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(item => new { item.Status, item.DiscoverabilityOptIn, item.EmailVerifiedAt });
                entity.HasIndex(item => item.SuspendedUntil);
            });

            builder.Entity<AllowedEmailDomain>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Domain).HasMaxLength(253).IsRequired();
                entity.HasIndex(item => item.Domain).IsUnique();
                entity.HasIndex(item => item.IsEnabled);
            });

            builder.Entity<UserProfileIntention>(entity =>
            {
                entity.HasKey(item => new { item.UserProfileId, item.UserIntentionId });
                entity.HasOne(item => item.UserProfile)
                    .WithMany(profile => profile.Intentions)
                    .HasForeignKey(item => item.UserProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.UserIntention)
                    .WithMany()
                    .HasForeignKey(item => item.UserIntentionId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(item => item.UserIntentionId);
            });

            builder.Entity<IntentCompatibilityRule>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.HasOne(item => item.SenderIntent)
                    .WithMany()
                    .HasForeignKey(item => item.SenderIntentId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.RecipientIntent)
                    .WithMany()
                    .HasForeignKey(item => item.RecipientIntentId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(item => new { item.SenderIntentId, item.RecipientIntentId }).IsUnique();
            });

            builder.Entity<Question>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.Code).HasMaxLength(128).IsRequired();
                entity.Property(item => item.PromptResourceKey).HasMaxLength(256).IsRequired();
                entity.Property(item => item.Group).HasMaxLength(64).IsRequired();
                entity.Property(item => item.QuestionVisibility).HasConversion<string>().HasMaxLength(32);
                entity.Property(item => item.ExplanationTemplateKey).HasMaxLength(256);
                entity.Property(item => item.ScoringWeight).HasPrecision(8, 5);
                entity.Property(item => item.UpdatedAt).IsRequired();
                entity.HasIndex(item => item.Code).IsUnique();
            });

            builder.Entity<QuestionOption>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.OptionCode).HasMaxLength(128).IsRequired();
                entity.Property(item => item.LabelResourceKey).HasMaxLength(256).IsRequired();
                entity.Property(item => item.ScoringValue).HasMaxLength(128);
                entity.HasOne(item => item.Question)
                    .WithMany(question => question.Options)
                    .HasForeignKey(item => item.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(item => new { item.QuestionId, item.OptionCode }).IsUnique();
                entity.HasIndex(item => new { item.QuestionId, item.SortOrder }).IsUnique();
            });

            builder.Entity<UserAnswer>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.ApplicationUserId).IsRequired();
                entity.HasOne(item => item.ApplicationUser)
                    .WithMany()
                    .HasForeignKey(item => item.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.QuestionOption)
                    .WithMany()
                    .HasForeignKey(item => item.QuestionOptionId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(item => new { item.ApplicationUserId, item.CreatedAt });
            });

            builder.Entity<RecommendationBatch>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.ViewerUserId).IsRequired();
                entity.Property(item => item.AlgorithmVersion).HasMaxLength(64).IsRequired();
                entity.Property(item => item.ScoringWeightsJson).HasColumnType("jsonb").IsRequired();
                entity.HasOne(item => item.ViewerUser)
                    .WithMany()
                    .HasForeignKey(item => item.ViewerUserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(item => new { item.ViewerUserId, item.BatchDate }).IsUnique();
                entity.HasIndex(item => item.CreatedAt);
            });

            builder.Entity<RecommendationItem>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.CandidateUserId).IsRequired();
                entity.Property(item => item.Decision).HasConversion<string>().HasMaxLength(32);
                entity.Property(item => item.EvidenceCoverage).HasPrecision(5, 4);
                entity.Property(item => item.CategoryScoresJson).HasColumnType("jsonb").IsRequired();
                entity.HasOne(item => item.RecommendationBatch)
                    .WithMany(batch => batch.Items)
                    .HasForeignKey(item => item.RecommendationBatchId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(item => new { item.RecommendationBatchId, item.CandidateUserId }).IsUnique();
                entity.HasIndex(item => new { item.CandidateUserId, item.Decision });
            });

            builder.Entity<RecommendationItem>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(item => item.CandidateUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ContactRequest>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.SenderUserId).IsRequired();
                entity.Property(item => item.RecipientUserId).IsRequired();
                entity.Property(item => item.State).HasConversion<string>().HasMaxLength(32);
                entity.HasOne(item => item.Conversation)
                    .WithMany()
                    .HasForeignKey(item => item.ConversationId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(item => new { item.SenderUserId, item.RecipientUserId, item.CreatedAt });
                entity.HasIndex(item => new { item.RecipientUserId, item.CreatedAt });
                entity.HasIndex(item => new { item.SenderUserId, item.RecipientUserId })
                    .IsUnique()
                    .HasFilter("\"State\" IN ('Pending', 'Accepted')");
                entity.HasOne(item => item.Sender)
                    .WithMany()
                    .HasForeignKey(item => item.SenderUserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Recipient)
                    .WithMany()
                    .HasForeignKey(item => item.RecipientUserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<UserIntention>()
                    .WithMany()
                    .HasForeignKey(item => item.SenderIntentId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<UserIntention>()
                    .WithMany()
                    .HasForeignKey(item => item.RecipientIntentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Conversation>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.FirstUserId).IsRequired();
                entity.Property(item => item.SecondUserId).IsRequired();
                entity.HasIndex(item => new { item.FirstUserId, item.SecondUserId }).IsUnique();
                entity.HasOne(item => item.FirstUser)
                    .WithMany()
                    .HasForeignKey(item => item.FirstUserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.SecondUser)
                    .WithMany()
                    .HasForeignKey(item => item.SecondUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ConversationMessage>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.SenderUserId).IsRequired();
                entity.Property(item => item.Body).HasMaxLength(4000).IsRequired();
                entity.HasOne(item => item.Conversation)
                    .WithMany(conversation => conversation.Messages)
                    .HasForeignKey(item => item.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(item => new { item.ConversationId, item.CreatedAt, item.Id });
                entity.HasOne(item => item.SenderUser)
                    .WithMany()
                    .HasForeignKey(item => item.SenderUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<UserBlock>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.BlockerUserId).IsRequired();
                entity.Property(item => item.BlockedUserId).IsRequired();
                entity.HasIndex(item => new { item.BlockerUserId, item.BlockedUserId }).IsUnique();
                entity.HasIndex(item => new { item.BlockedUserId, item.BlockerUserId });
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(item => item.BlockerUserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(item => item.BlockedUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserReport>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.ReporterUserId).IsRequired();
                entity.Property(item => item.ReportedUserId).IsRequired();
                entity.Property(item => item.Reason).HasConversion<string>().HasMaxLength(64);
                entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
                entity.Property(item => item.Description).HasMaxLength(2000);
                entity.HasIndex(item => new { item.Status, item.CreatedAt });
                entity.HasIndex(item => new { item.ReportedUserId, item.CreatedAt });
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(item => item.ReporterUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(item => item.ReportedUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ModerationAction>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.ModeratorUserId).IsRequired();
                entity.Property(item => item.Action).HasConversion<string>().HasMaxLength(64);
                entity.Property(item => item.Notes).HasMaxLength(2000);
                entity.HasOne(item => item.Report)
                    .WithMany()
                    .HasForeignKey(item => item.ReportId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(item => new { item.ReportId, item.CreatedAt });
                entity.HasIndex(item => new { item.ModeratorUserId, item.CreatedAt });
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(item => item.ModeratorUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AuditEvent>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.ActorUserId).IsRequired();
                entity.Property(item => item.Action).HasMaxLength(128).IsRequired();
                entity.Property(item => item.EntityType).HasMaxLength(128).IsRequired();
                entity.Property(item => item.EntityId).HasMaxLength(128).IsRequired();
                entity.Property(item => item.DetailsJson).HasColumnType("jsonb");
                entity.HasIndex(item => new { item.EntityType, item.EntityId, item.OccurredAt });
                entity.HasIndex(item => new { item.OccurredAt });
                entity.HasOne(item => item.ApplicationUser)
                    .WithMany()
                    .HasForeignKey(item => item.ActorUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}