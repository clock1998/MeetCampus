using MeetCampus.Data.Profiles;
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

            builder.Entity<UserProfileImage>(entity =>
            {
                entity.HasKey(item => item.Id);
                entity.Property(item => item.ImageKey).HasMaxLength(256);
                entity.HasOne(image => image.UserProfile)
                    .WithMany(profile => profile.ProfileImages)
                    .HasForeignKey(image => image.UserProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}