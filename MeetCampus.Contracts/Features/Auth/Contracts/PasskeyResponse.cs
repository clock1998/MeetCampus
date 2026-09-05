namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record PasskeyResponse(
    string CredentialId,
    string? Name);
