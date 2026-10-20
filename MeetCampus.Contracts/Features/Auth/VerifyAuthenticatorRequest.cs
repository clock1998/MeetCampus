namespace MeetCampus.Contracts.Features.Auth;

public sealed record VerifyAuthenticatorRequest(
    string Code);
