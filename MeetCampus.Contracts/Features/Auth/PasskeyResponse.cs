namespace MeetCampus.Contracts.Features.Auth;

public sealed record PasskeyResponse(
    string CredentialId,
    string? Name);
