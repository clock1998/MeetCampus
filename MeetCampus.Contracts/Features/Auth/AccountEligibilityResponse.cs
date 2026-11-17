namespace MeetCampus.Contracts.Features.Auth;

public sealed record AccountEligibilityResponse(AccountEligibilityFailure Failures)
{
    public bool IsEligibleForDiscoverability => Failures == AccountEligibilityFailure.None;
}
