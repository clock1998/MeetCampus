using MeetCampus.Contracts.Features.Auth;
using MeetCampus.Data;
using MeetCampus.Data.Contact;
using MeetCampus.Data.Safety;
using MeetCampus.Data.User;
using MeetCampus.Features.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MeetCampus.Features.Auth.Services;

public sealed class AccountEligibilityService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IOptions<ContactOptions> contactOptions)
{
    public async Task<AccountEligibilityResponse?> GetEligibilityAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        var lifecycle = await dbContext.AccountLifecycles
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.ApplicationUserId == userId, cancellationToken);
        var profile = await dbContext.UserProfiles
            .AsNoTracking()
            .Include(item => item.Intentions)
            .SingleOrDefaultAsync(item => item.ApplicationUserId == userId, cancellationToken);

        var isProfileComplete = profile is not null
            && profile.StudyDomainId != Guid.Empty
            && profile.SchoolId != Guid.Empty
            && profile.LanguageId != Guid.Empty
            && profile.EthnicityId != Guid.Empty
            && profile.Intentions.Count > 0;
        var hasUploadedProfileImage = profile is not null && await dbContext.UserProfileImages
            .AsNoTracking()
            .AnyAsync(item => item.UserProfileId == profile.Id, cancellationToken);

        var requiredQuestions = dbContext.QuestionDefinitions
            .AsNoTracking()
            .Where(question => question.IsRequired);
        var requiredQuestionCount = await requiredQuestions.CountAsync(cancellationToken);
        var answeredRequiredQuestionCount = await dbContext.QuestionnaireAnswerSets
            .AsNoTracking()
            .Where(answer => answer.ApplicationUserId == userId)
            .Select(answer => answer.QuestionOption.QuestionId)
            .Distinct()
            .CountAsync(questionId => requiredQuestions.Any(question => question.Id == questionId), cancellationToken);
        var hasCompletedRequiredQuestions = answeredRequiredQuestionCount == requiredQuestionCount;

        var isEmailVerified = await userManager.IsEmailConfirmedAsync(user);
        var discoverabilityOptIn = lifecycle?.DiscoverabilityOptIn == true;
        var now = DateTimeOffset.UtcNow;
        var isActive = lifecycle?.Status == AccountStatus.Active;
        var hasRecipientCapacity = await dbContext.ContactRequests
            .AsNoTracking()
            .CountAsync(request => request.RecipientUserId == userId
                && request.CreatedAt >= now.AddDays(-7), cancellationToken)
            < contactOptions.Value.MaxInboundPerRolling7Days;
        var hasNoActiveModerationRestriction = !await dbContext.ModerationActions
            .AsNoTracking()
            .AnyAsync(action => action.Report.ReportedUserId == userId
                && (action.Action == ModerationActionType.PermanentRemoval
                    || action.Action == ModerationActionType.TemporarySuspension || action.Action == ModerationActionType.MessagingRestriction
                        && (action.EndsAt == null || action.EndsAt > now)), cancellationToken);
        var isEligible = isActive
            && isEmailVerified
            && discoverabilityOptIn
            && isProfileComplete
            && hasUploadedProfileImage
            && hasCompletedRequiredQuestions
            && hasRecipientCapacity
            && hasNoActiveModerationRestriction;

        return new AccountEligibilityResponse(
            isEmailVerified,
            isActive,
            isProfileComplete,
            hasUploadedProfileImage,
            hasCompletedRequiredQuestions,
            discoverabilityOptIn,
            hasRecipientCapacity,
            hasNoActiveModerationRestriction,
            isEligible);
    }
}
