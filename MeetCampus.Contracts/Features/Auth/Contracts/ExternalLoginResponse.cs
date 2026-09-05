namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record ExternalLoginResponse(
    IReadOnlyList<ExternalLoginItem> CurrentLogins,
    IReadOnlyList<ExternalProviderItem> AvailableProviders,
    bool CanRemoveLogin);
