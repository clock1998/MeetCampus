namespace MeetCampus.Contracts.Features.Auth;

public sealed record UpdateAccountLifecycleRequest(
    AccountLifecycleAction Action,
    bool? DiscoverabilityOptIn = null);
