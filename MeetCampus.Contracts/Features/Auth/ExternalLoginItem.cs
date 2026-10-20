namespace MeetCampus.Contracts.Features.Auth;

public sealed record ExternalLoginItem(
    string LoginProvider,
    string ProviderKey,
    string ProviderDisplayName);
