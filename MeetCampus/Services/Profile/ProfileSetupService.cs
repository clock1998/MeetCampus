using MeetCampus.Data;
using MeetCampus.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Services.Profile;

public sealed class ProfileSetupService(ApplicationDbContext dbContext)
{
    public async Task<UserProfileSetupResponse> GetSetupAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID must not be empty.", nameof(userId));

        var profile = await dbContext.UserProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.ApplicationUserId == userId, cancellationToken);

        return new UserProfileSetupResponse
        {
            Profile = new UserProfileSetupForm
            {
                GenderId = profile?.GenderId,
                SexualityId = profile?.SexualityId,
                StudyDomainId = profile?.StudyDomainId,
                SchoolId = profile?.SchoolId,
                LanguageId = profile?.LanguageId,
                IntentionId = profile?.IntentionId,
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
    public async Task UpdateSetupAsync(string userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID must not be empty.", nameof(userId));
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

        if (!await dbContext.UserIntentions.AnyAsync(i => i.Id == request.IntentionId, cancellationToken))
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
        profile.IntentionId = request.IntentionId;
        profile.EthnicityId = request.EthnicityId;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
