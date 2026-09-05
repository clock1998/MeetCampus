namespace MeetCampus.Features.Auth.Models;

internal sealed record LoginAttempt(
    string Email,
    string Password,
    bool RememberMe,
    string? PasskeyCredentialJson,
    string? PasskeyError);
