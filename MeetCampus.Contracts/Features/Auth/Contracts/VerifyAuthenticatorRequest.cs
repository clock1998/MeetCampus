namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record VerifyAuthenticatorRequest(
    string Code);
