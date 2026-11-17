using MeetCampus.Contracts.Features.Auth;
using MeetCampus.Data;
using MeetCampus.Data.User;
using MeetCampus.Features.Account.Services;
using MeetCampus.Features.Matching.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Features.Matching.Services;

public sealed class MatchEligibilityService(ApplicationDbContext dbContext, AccountEligibilityService accountEligibilityService)
{
    /// <summary>Checks whether two members can be considered for a match at the time of the request.</summary>
    public async Task<bool> IsEligibleForMatchingAsync(ApplicationUser firstUser, ApplicationUser secondUser, CancellationToken cancellationToken = default) =>
        await GetStatusAsync(firstUser, secondUser, cancellationToken) == MatchEvaluationStatus.Completed;

    internal async Task<MatchEvaluationStatus> GetStatusAsync(Guid firstUserId, Guid secondUserId, CancellationToken cancellationToken = default)
    {
        if (firstUserId == Guid.Empty)
            throw new ArgumentException("A user ID is required.", nameof(firstUserId));
        if (secondUserId == Guid.Empty)
            throw new ArgumentException("A user ID is required.", nameof(secondUserId));
        if (firstUserId == secondUserId)
            return MatchEvaluationStatus.Ineligible;

        var users = await dbContext.Users.AsNoTracking()
            .Where(user => user.Id == firstUserId || user.Id == secondUserId)
            .ToListAsync(cancellationToken);
        var firstUser = users.SingleOrDefault(user => user.Id == firstUserId);
        var secondUser = users.SingleOrDefault(user => user.Id == secondUserId);
        if (firstUser is null || secondUser is null)
            return MatchEvaluationStatus.Ineligible;

        return await GetStatusAsync(firstUser, secondUser, cancellationToken);
    }

    private async Task<MatchEvaluationStatus> GetStatusAsync(ApplicationUser firstUser, ApplicationUser secondUser, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(firstUser);
        ArgumentNullException.ThrowIfNull(secondUser);
        var firstUserId = firstUser.Id;
        var secondUserId = secondUser.Id;
        if (firstUserId == Guid.Empty)
            throw new ArgumentException("A user ID is required.", nameof(firstUser));
        if (secondUserId == Guid.Empty)
            throw new ArgumentException("A user ID is required.", nameof(secondUser));
        if (firstUserId == secondUserId)
            return MatchEvaluationStatus.Ineligible;

        var firstEligibility = await accountEligibilityService.GetEligibilityAsync(firstUser, cancellationToken);
        var secondEligibility = await accountEligibilityService.GetEligibilityAsync(secondUser, cancellationToken);

        var firstFailures = firstEligibility.Failures;
        var secondFailures = secondEligibility.Failures;
        if (((firstFailures | secondFailures) & ~AccountEligibilityFailure.AnswersIncomplete) != AccountEligibilityFailure.None)
            return MatchEvaluationStatus.Ineligible;

        var blocked = await dbContext.UserBlocks.AsNoTracking().AnyAsync(
            block => (block.BlockerUserId == firstUserId && block.BlockedUserId == secondUserId)
                || (block.BlockerUserId == secondUserId && block.BlockedUserId == firstUserId), cancellationToken);
        if (blocked)
            return MatchEvaluationStatus.Ineligible;

        var intentions = await dbContext.UserProfileIntentions.AsNoTracking()
            .Where(item => item.UserProfile.ApplicationUserId == firstUserId || item.UserProfile.ApplicationUserId == secondUserId)
            .Select(item => new { UserId = item.UserProfile.ApplicationUserId, Code = item.UserIntention.Code })
            .ToListAsync(cancellationToken);

        if (!AreIntentsCompatible(
            intentions.Where(item => item.UserId == firstUserId).Select(item => item.Code).ToArray(),
            intentions.Where(item => item.UserId == secondUserId).Select(item => item.Code).ToArray()))
            return MatchEvaluationStatus.Ineligible;

        return (firstFailures.HasFlag(AccountEligibilityFailure.AnswersIncomplete), secondFailures.HasFlag(AccountEligibilityFailure.AnswersIncomplete)) switch
        {
            (true, true) => MatchEvaluationStatus.BothMembersAnswersIncomplete,
            (true, false) => MatchEvaluationStatus.FirstMemberAnswersIncomplete,
            (false, true) => MatchEvaluationStatus.SecondMemberAnswersIncomplete,
            _ => MatchEvaluationStatus.Completed
        };
    }

    private bool AreIntentsCompatible(IReadOnlyCollection<string> firstIntents, IReadOnlyCollection<string> secondIntents)
    {
        ArgumentNullException.ThrowIfNull(firstIntents);
        ArgumentNullException.ThrowIfNull(secondIntents);

        foreach (var intent in firstIntents.Concat(secondIntents))
        {
            if (intent is not ("friendship" or "dating" or "study-partner" or "activity-partner"))
                throw new InvalidOperationException("A selected intent does not have a compatibility rule.");
        }

        return HasAllowedPair(firstIntents, secondIntents) && HasAllowedPair(secondIntents, firstIntents);
    }

    private bool HasAllowedPair(IEnumerable<string> senderIntents, IReadOnlyCollection<string> recipientIntents) =>
        senderIntents.Any(sender => recipientIntents.Any(recipient => IsAllowed(sender, recipient)));

    private bool IsAllowed(string sender, string recipient) => (sender, recipient) switch
    {
        ("friendship", "friendship") or ("dating", "dating")
            or ("study-partner", "study-partner") or ("activity-partner", "activity-partner") => true,
        _ => false
    };
}
