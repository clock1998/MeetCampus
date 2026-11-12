using MeetCampus.Configuration;
using MeetCampus.Contracts.Features.Auth;
using MeetCampus.Data;
using MeetCampus.Data.Contact;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.Safety;
using MeetCampus.Data.User;
using MeetCampus.Features.Questionnaire.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MeetCampus.Features.Account.Services;

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
            return null;

        var lifecycle = await GetLifecycleAsync(userId, cancellationToken);
        var profile = await GetProfileAsync(userId, cancellationToken);
        var isProfileComplete = IsProfileComplete(profile);
        var hasUploadedProfileImage = await HasUploadedProfileImageAsync(profile, cancellationToken);
        var hasCompletedRequiredQuestions = await HasCompletedRequiredQuestionsAsync(userId, cancellationToken);
        var isEmailVerified = await userManager.IsEmailConfirmedAsync(user);
        var discoverabilityOptIn = IsDiscoverabilityOptedIn(lifecycle);
        var now = DateTimeOffset.UtcNow;
        var isActive = IsAccountActive(lifecycle);
        var hasRecipientCapacity = await HasRecipientCapacityAsync(userId, now, cancellationToken);
        var hasNoActiveModerationRestriction = await HasNoActiveModerationRestrictionAsync(userId, now, cancellationToken);
        var isEligible = IsEligibleForDiscoverability(
            isActive,
            isEmailVerified,
            discoverabilityOptIn,
            isProfileComplete,
            hasUploadedProfileImage,
            hasCompletedRequiredQuestions,
            hasRecipientCapacity,
            hasNoActiveModerationRestriction);

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

    private Task<AccountLifecycle?> GetLifecycleAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.AccountLifecycles
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.ApplicationUserId == userId, cancellationToken);

    private Task<UserProfile?> GetProfileAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.UserProfiles
            .AsNoTracking()
            .Include(item => item.Intentions)
            .SingleOrDefaultAsync(item => item.ApplicationUserId == userId, cancellationToken);

    private static bool IsProfileComplete(UserProfile? profile) => profile is not null
            && profile.StudyDomainId != Guid.Empty
            && profile.SchoolId != Guid.Empty
            && profile.LanguageId != Guid.Empty
            && profile.EthnicityId != Guid.Empty
            && profile.Intentions.Count > 0;

    private Task<bool> HasUploadedProfileImageAsync(UserProfile? profile, CancellationToken cancellationToken) =>
        profile is null
            ? Task.FromResult(false)
            : dbContext.UserProfileImages
            .AsNoTracking()
            .AnyAsync(item => item.UserProfileId == profile.Id, cancellationToken);

    private async Task<bool> HasCompletedRequiredQuestionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var questions = await dbContext.Questions
            .AsNoTracking()
            .Include(question => question.Options)
            .ToListAsync(cancellationToken);
        var questionIds = questions.Select(question => question.Id).ToArray();
        var requiredAnswersByQuestionId = await dbContext.UserAnswers
            .AsNoTracking()
            .Where(answer => answer.ApplicationUserId == userId
                && questionIds.Contains(answer.QuestionId))
            .ToListAsync(cancellationToken);
        var answersByQuestionId = requiredAnswersByQuestionId
            .GroupBy(answer => answer.QuestionId)
            .ToDictionary(group => group.Key, group => (IReadOnlyCollection<UserAnswer>)group.ToList());

        return questions.All(question =>
            answersByQuestionId.TryGetValue(question.Id, out var answers)
            && answers.All(answer => answer.UpdatedAt >= question.UpdatedAt)
            && QuestionnaireAnswerRules.IsValidStoredAnswer(question, answers));
    }

    private static bool IsDiscoverabilityOptedIn(AccountLifecycle? lifecycle) =>
        lifecycle?.DiscoverabilityOptIn == true;

    private static bool IsAccountActive(AccountLifecycle? lifecycle) =>
        lifecycle?.Status == AccountStatus.Active;

    private async Task<bool> HasRecipientCapacityAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken) =>
        await dbContext.ContactRequests
            .AsNoTracking()
            .CountAsync(request => request.RecipientUserId == userId
                && request.CreatedAt >= now.AddDays(-7), cancellationToken)
            < contactOptions.Value.MaxInboundPerRolling7Days;

    private async Task<bool> HasNoActiveModerationRestrictionAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken) =>
        !await dbContext.ModerationActions
            .AsNoTracking()
            .AnyAsync(action => action.Report.ReportedUserId == userId
                && (action.Action == ModerationActionType.PermanentRemoval
                    || (action.Action == ModerationActionType.TemporarySuspension || action.Action == ModerationActionType.MessagingRestriction)
                        && (action.EndsAt == null || action.EndsAt > now)), cancellationToken);

    private static bool IsEligibleForDiscoverability(
        bool isActive,
        bool isEmailVerified,
        bool discoverabilityOptIn,
        bool isProfileComplete,
        bool hasUploadedProfileImage,
        bool hasCompletedRequiredQuestions,
        bool hasRecipientCapacity,
        bool hasNoActiveModerationRestriction) =>
        isActive
            && isEmailVerified
            && discoverabilityOptIn
            && isProfileComplete
            && hasUploadedProfileImage
            && hasCompletedRequiredQuestions
            && hasRecipientCapacity
            && hasNoActiveModerationRestriction;
}
