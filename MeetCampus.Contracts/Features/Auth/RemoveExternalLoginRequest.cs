namespace MeetCampus.Contracts.Features.Auth;

public sealed record RemoveExternalLoginRequest(
    string LoginProvider,
    string ProviderKey);