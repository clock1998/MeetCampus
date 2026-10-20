namespace MeetCampus.Contracts.Features.Auth;

public sealed record ExternalLoginResponse(
    IReadOnlyList<ExternalLoginItem> CurrentLogins,
    IReadOnlyList<ExternalProviderItem> AvailableProviders,
    bool CanRemoveLogin);
