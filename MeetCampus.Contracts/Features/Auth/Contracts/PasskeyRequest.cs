namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record PasskeyRequest(
    string CredentialId,
    string? Name = null,
    string? CredentialJson = null);