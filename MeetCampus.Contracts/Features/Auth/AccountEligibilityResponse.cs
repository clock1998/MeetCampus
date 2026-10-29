namespace MeetCampus.Contracts.Features.Auth;

public sealed record AccountEligibilityResponse(
    bool IsEmailVerified,
    bool IsAccountActive,
    bool IsProfileComplete,
    bool HasUploadedProfileImage,
    bool HasCompletedRequiredQuestions,
    bool DiscoverabilityOptIn,
    bool HasRecipientCapacity,
    bool HasNoActiveModerationRestriction,
    bool IsEligibleForDiscoverability);
