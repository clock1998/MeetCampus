using MeetCampus.Data;
using MeetCampus.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MeetCampus.Endpoints;

internal static class ProfileApiHandlers
{
    public static async Task<IResult> GetProfileSetupAsync(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(dbContext);

        var userId = userManager.GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        var profile = await dbContext.UserProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.ApplicationUserId == userId, cancellationToken);

        var response = new UserProfileSetupResponse
        {
            Profile = new UserProfileSetupForm
            {
                Gender = profile?.Gender,
                Sexuality = profile?.Sexuality,
                StudyDomainId = profile?.StudyDomainId,
                SchoolId = profile?.SchoolId,
                LanguageId = profile?.LanguageId,
                IntentionId = profile?.IntentionId,
                EthnicityId = profile?.EthnicityId,
            },
            StudyDomains = await dbContext.StudyDomains
                .AsNoTracking()
                .OrderBy(item => item.Name)
                .Select(item => new ProfileLookupOption(item.Id, null, item.Name))
                .ToListAsync(cancellationToken),
            Schools = await dbContext.Schools
                .AsNoTracking()
                .OrderBy(item => item.Name)
                .Select(item => new ProfileLookupOption(item.Id, null, item.Name))
                .ToListAsync(cancellationToken),
            Languages = await dbContext.CampusLanguages
                .AsNoTracking()
                .OrderBy(item => item.Name)
                .Select(item => new ProfileLookupOption(item.Id, null, item.Name))
                .ToListAsync(cancellationToken),
            Intentions = await dbContext.UserIntentions
                .AsNoTracking()
                .OrderBy(item => item.Name)
                .Select(item => new ProfileLookupOption(item.Id, item.DisplayKey, item.Name))
                .ToListAsync(cancellationToken),
            Ethnicities = await dbContext.UserEthnicities
                .AsNoTracking()
                .OrderBy(item => item.Name)
                .Select(item => new ProfileLookupOption(item.Id, item.DisplayKey, item.Name))
                .ToListAsync(cancellationToken)
        };

        return Results.Ok(response);
    }

    public static async Task<IResult> UpdateUserProfileAsync(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(request);

        var userId = userManager.GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        if (!await dbContext.StudyDomains.AnyAsync(item => item.Id == request.StudyDomainId, cancellationToken))
        {
            return Results.BadRequest("Invalid study domain.");
        }

        if (!await dbContext.Schools.AnyAsync(item => item.Id == request.SchoolId, cancellationToken))
        {
            return Results.BadRequest("Invalid school.");
        }

        if (!await dbContext.CampusLanguages.AnyAsync(item => item.Id == request.LanguageId, cancellationToken))
        {
            return Results.BadRequest("Invalid language.");
        }

        if (!await dbContext.UserIntentions.AnyAsync(item => item.Id == request.IntentionId, cancellationToken))
        {
            return Results.BadRequest("Invalid intention.");
        }

        if (!await dbContext.UserEthnicities.AnyAsync(item => item.Id == request.EthnicityId, cancellationToken))
        {
            return Results.BadRequest("Invalid ethnicity.");
        }

        var profile = await dbContext.UserProfiles
            .SingleOrDefaultAsync(item => item.ApplicationUserId == userId, cancellationToken);

        if (profile is null)
        {
            profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = userId
            };

            dbContext.UserProfiles.Add(profile);
        }

        profile.Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim();
        profile.Sexuality = string.IsNullOrWhiteSpace(request.Sexuality) ? null : request.Sexuality.Trim();
        profile.StudyDomainId = request.StudyDomainId;
        profile.SchoolId = request.SchoolId;
        profile.LanguageId = request.LanguageId;
        profile.IntentionId = request.IntentionId;
        profile.EthnicityId = request.EthnicityId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
