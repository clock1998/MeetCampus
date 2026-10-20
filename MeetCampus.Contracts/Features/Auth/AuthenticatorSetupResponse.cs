namespace MeetCampus.Contracts.Features.Auth;

public sealed record AuthenticatorSetupResponse(
    string SharedKey,
    string AuthenticatorUri);
