namespace MeetCampus.Contracts.Features.Auth;

public sealed record LoginRequest(
    string Email,
    string Password,
    bool RememberMe);
