namespace MeetCampus.Contracts.Features.Auth;

public sealed record PasskeyRequest(
    string CredentialId,
    string? Name = null,
    string? CredentialJson = null);