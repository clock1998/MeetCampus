namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record ExternalLoginItem(
    string LoginProvider,
    string ProviderKey,
    string ProviderDisplayName);
