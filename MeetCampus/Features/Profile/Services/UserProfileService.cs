using MeetCampus.Contracts.Features.Profile;
using MeetCampus.Data;
using MeetCampus.Data.Profiles;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Features.Profile.Services;

public sealed class UserProfileService(ApplicationDbContext dbContext, ProfileImageService profileImageService)
{
    public async Task<UserProfileSetupResponse> GetProfileSetupAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.UserProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.ApplicationUserId == userId, cancellationToken);

        return new UserProfileSetupResponse
        {
            ProfileImages = await profileImageService.GetProfileImagesAsync(userId, cancellationToken),
            Profile = new UserProfileSetupForm
            {
                GenderId = profile?.GenderId,
                SexualityId = profile?.SexualityId,
                StudyDomainId = profile?.StudyDomainId,
                SchoolId = profile?.SchoolId,
                LanguageId = profile?.LanguageId,
                IntentionIds = profile is null
                    ? []
                    : await dbContext.UserProfileIntentions
                        .AsNoTracking()
                        .Where(item => item.UserProfileId == profile.Id)
                        .Select(item => item.UserIntentionId)
                        .ToListAsync(cancellationToken),
                EthnicityId = profile?.EthnicityId,
            },
            Genders = await dbContext.Genders
                .AsNoTracking()
                .OrderBy(g => g.Name)
                .Select(g => new ProfileLookupOption(g.Id, g.DisplayKey, g.Name))
                .ToListAsync(cancellationToken),
            Sexualities = await dbContext.Sexualities
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new ProfileLookupOption(s.Id, s.DisplayKey, s.Name))
                .ToListAsync(cancellationToken),
            StudyDomains = await dbContext.StudyDomains
                .AsNoTracking()
                .OrderBy(d => d.Name)
                .Select(d => new ProfileLookupOption(d.Id, null, d.Name))
                .ToListAsync(cancellationToken),
            Schools = await dbContext.Schools
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new ProfileLookupOption(s.Id, null, s.Name))
                .ToListAsync(cancellationToken),
            Languages = await dbContext.CampusLanguages
                .AsNoTracking()
                .OrderBy(l => l.Name)
                .Select(l => new ProfileLookupOption(l.Id, null, l.Name))
                .ToListAsync(cancellationToken),
            Intentions = await dbContext.UserIntentions
                .AsNoTracking()
                .OrderBy(i => i.Name)
                .Select(i => new ProfileLookupOption(i.Id, i.DisplayKey, i.Name))
                .ToListAsync(cancellationToken),
            Ethnicities = await dbContext.UserEthnicities
                .AsNoTracking()
                .OrderBy(e => e.Name)
                .Select(e => new ProfileLookupOption(e.Id, e.DisplayKey, e.Name))
                .ToListAsync(cancellationToken),
        };
    }

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> when a referenced lookup ID does not exist.
    /// </summary>
    public async Task UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.GenderId.HasValue && !await dbContext.Genders.AnyAsync(g => g.Id == request.GenderId.Value, cancellationToken))
            throw new InvalidOperationException("Invalid gender.");

        if (request.SexualityId.HasValue && !await dbContext.Sexualities.AnyAsync(s => s.Id == request.SexualityId.Value, cancellationToken))
            throw new InvalidOperationException("Invalid sexuality.");

        if (!await dbContext.StudyDomains.AnyAsync(d => d.Id == request.StudyDomainId, cancellationToken))
            throw new InvalidOperationException("Invalid study domain.");

        if (!await dbContext.Schools.AnyAsync(s => s.Id == request.SchoolId, cancellationToken))
            throw new InvalidOperationException("Invalid school.");

        if (!await dbContext.CampusLanguages.AnyAsync(l => l.Id == request.LanguageId, cancellationToken))
            throw new InvalidOperationException("Invalid language.");

        if (request.IntentionIds.Count == 0 || request.IntentionIds.Distinct().Count() != request.IntentionIds.Count)
            throw new InvalidOperationException("Select one or more unique intentions.");

        var validIntentionCount = await dbContext.UserIntentions
            .CountAsync(intention => request.IntentionIds.Contains(intention.Id), cancellationToken);
        if (validIntentionCount != request.IntentionIds.Count)
            throw new InvalidOperationException("Invalid intention.");

        if (!await dbContext.UserEthnicities.AnyAsync(e => e.Id == request.EthnicityId, cancellationToken))
            throw new InvalidOperationException("Invalid ethnicity.");

        var profile = await dbContext.UserProfiles
            .SingleOrDefaultAsync(p => p.ApplicationUserId == userId, cancellationToken);

        if (profile is null)
        {
            profile = new UserProfile { Id = Guid.NewGuid(), ApplicationUserId = userId };
            dbContext.UserProfiles.Add(profile);
        }

        profile.GenderId = request.GenderId;
        profile.SexualityId = request.SexualityId;
        profile.StudyDomainId = request.StudyDomainId;
        profile.SchoolId = request.SchoolId;
        profile.LanguageId = request.LanguageId;
        profile.EthnicityId = request.EthnicityId;

        var existingIntentions = await dbContext.UserProfileIntentions
            .Where(item => item.UserProfileId == profile.Id)
            .ToListAsync(cancellationToken);
        dbContext.UserProfileIntentions.RemoveRange(existingIntentions);
        foreach (var intentionId in request.IntentionIds)
        {
            dbContext.UserProfileIntentions.Add(new UserProfileIntention
            {
                UserProfileId = profile.Id,
                UserIntentionId = intentionId
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
