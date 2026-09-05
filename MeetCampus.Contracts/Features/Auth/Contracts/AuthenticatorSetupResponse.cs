namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record AuthenticatorSetupResponse(
    string SharedKey,
    string AuthenticatorUri);
