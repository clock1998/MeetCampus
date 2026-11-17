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
    /// <summary>Checks all requirements for an account's discoverability.</summary>
    public async Task<AccountEligibilityResponse> GetEligibilityAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        var userId = user.Id;

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

        var failures = AccountEligibilityFailure.None;
        if (!isEmailVerified)
            failures |= AccountEligibilityFailure.EmailNotVerified;
        if (!isActive)
            failures |= AccountEligibilityFailure.AccountNotActive;
        if (!isProfileComplete)
            failures |= AccountEligibilityFailure.ProfileIncomplete;
        if (!hasUploadedProfileImage)
            failures |= AccountEligibilityFailure.ProfileImageMissing;
        if (!hasCompletedRequiredQuestions)
            failures |= AccountEligibilityFailure.AnswersIncomplete;
        if (!discoverabilityOptIn)
            failures |= AccountEligibilityFailure.DiscoverabilityNotOptedIn;
        if (!hasRecipientCapacity)
            failures |= AccountEligibilityFailure.RecipientCapacityReached;
        if (!hasNoActiveModerationRestriction)
            failures |= AccountEligibilityFailure.ModerationRestriction;

        return new AccountEligibilityResponse(failures);
    }

    /// <summary>Loads the account lifecycle for a member.</summary>
    public Task<AccountLifecycle?> GetLifecycleAsync(Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.AccountLifecycles
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.ApplicationUserId == userId, cancellationToken);

    /// <summary>Loads a member's profile and selected intentions.</summary>
    public Task<UserProfile?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.UserProfiles
            .AsNoTracking()
            .Include(item => item.Intentions)
            .SingleOrDefaultAsync(item => item.ApplicationUserId == userId, cancellationToken);

    /// <summary>Checks the required profile details and intentions.</summary>
    public bool IsProfileComplete(UserProfile? profile) => profile is not null
            && profile.StudyDomainId != Guid.Empty
            && profile.SchoolId != Guid.Empty
            && profile.LanguageId != Guid.Empty
            && profile.EthnicityId != Guid.Empty
            && profile.Intentions.Count > 0;

    /// <summary>Checks whether the profile has an uploaded image.</summary>
    public Task<bool> HasUploadedProfileImageAsync(UserProfile? profile, CancellationToken cancellationToken = default) =>
        profile is null
            ? Task.FromResult(false)
            : dbContext.UserProfileImages
            .AsNoTracking()
            .AnyAsync(item => item.UserProfileId == profile.Id, cancellationToken);

    /// <summary>Checks that all required questions have valid, current answers.</summary>
    public async Task<bool> HasCompletedRequiredQuestionsAsync(Guid userId, CancellationToken cancellationToken = default)
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

    /// <summary>Checks whether the account opted in to discoverability.</summary>
    public bool IsDiscoverabilityOptedIn(AccountLifecycle? lifecycle) =>
        lifecycle?.DiscoverabilityOptIn == true;

    /// <summary>Checks whether the account is active.</summary>
    public bool IsAccountActive(AccountLifecycle? lifecycle) =>
        lifecycle?.Status == AccountStatus.Active;

    /// <summary>Checks the member's inbound contact capacity at the specified time.</summary>
    public async Task<bool> HasRecipientCapacityAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken = default) =>
        await dbContext.ContactRequests
            .AsNoTracking()
            .CountAsync(request => request.RecipientUserId == userId
                && request.CreatedAt >= now.AddDays(-7), cancellationToken)
            < contactOptions.Value.MaxInboundPerRolling7Days;

    /// <summary>Checks whether the member has no applicable moderation restriction at the specified time.</summary>
    public async Task<bool> HasNoActiveModerationRestrictionAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken = default) =>
        !await dbContext.ModerationActions
            .AsNoTracking()
            .AnyAsync(action => action.Report.ReportedUserId == userId
                && (action.Action == ModerationActionType.PermanentRemoval
                    || (action.Action == ModerationActionType.TemporarySuspension || action.Action == ModerationActionType.MessagingRestriction)
                        && (action.EndsAt == null || action.EndsAt > now)), cancellationToken);

}
