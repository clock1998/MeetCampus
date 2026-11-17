namespace MeetCampus.Contracts.Features.Auth;

[Flags]
public enum AccountEligibilityFailure
{
    None = 0,
    EmailNotVerified = 1 << 0,
    AccountNotActive = 1 << 1,
    ProfileIncomplete = 1 << 2,
    ProfileImageMissing = 1 << 3,
    AnswersIncomplete = 1 << 4,
    DiscoverabilityNotOptedIn = 1 << 5,
    RecipientCapacityReached = 1 << 6,
    ModerationRestriction = 1 << 7
}
