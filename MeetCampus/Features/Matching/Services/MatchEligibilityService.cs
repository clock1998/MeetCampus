using MeetCampus.Data;
using MeetCampus.Features.Account.Services;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Features.Matching.Services;

public sealed class MatchEligibilityService(ApplicationDbContext dbContext, AccountEligibilityService accountEligibilityService)
{
    /// <summary>Checks whether two members can be considered for a match at the time of the request.</summary>
    public async Task<bool> CanMatchAsync(Guid firstUserId, Guid secondUserId, CancellationToken cancellationToken = default)
    {
        if (firstUserId == Guid.Empty)
            throw new ArgumentException("A user ID is required.", nameof(firstUserId));
        if (secondUserId == Guid.Empty)
            throw new ArgumentException("A user ID is required.", nameof(secondUserId));
        if (firstUserId == secondUserId)
            return false;

        var firstEligibility = await accountEligibilityService.GetEligibilityAsync(firstUserId, cancellationToken);
        if (firstEligibility?.IsEligibleForDiscoverability != true)
            return false;

        var secondEligibility = await accountEligibilityService.GetEligibilityAsync(secondUserId, cancellationToken);
        if (secondEligibility?.IsEligibleForDiscoverability != true)
            return false;

        var blocked = await dbContext.UserBlocks.AsNoTracking().AnyAsync(
            block => (block.BlockerUserId == firstUserId && block.BlockedUserId == secondUserId)
                || (block.BlockerUserId == secondUserId && block.BlockedUserId == firstUserId), cancellationToken);
        if (blocked)
            return false;

        var intentions = await dbContext.UserProfileIntentions.AsNoTracking()
            .Where(item => item.UserProfile.ApplicationUserId == firstUserId || item.UserProfile.ApplicationUserId == secondUserId)
            .Select(item => new { UserId = item.UserProfile.ApplicationUserId, Code = item.UserIntention.Code })
            .ToListAsync(cancellationToken);

        return IntentCompatibilityService.AreCompatible(
            intentions.Where(item => item.UserId == firstUserId).Select(item => item.Code).ToArray(),
            intentions.Where(item => item.UserId == secondUserId).Select(item => item.Code).ToArray());
    }
}
