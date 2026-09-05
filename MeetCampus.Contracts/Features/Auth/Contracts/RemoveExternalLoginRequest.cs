namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record RemoveExternalLoginRequest(
    string LoginProvider,
    string ProviderKey);