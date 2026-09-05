namespace MeetCampus.Features.Auth.Models;

public sealed record LoginAttempt(
    string Email,
    string Password,
    bool RememberMe,
    string? PasskeyCredentialJson,
    string? PasskeyError);
