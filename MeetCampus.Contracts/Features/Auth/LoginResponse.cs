namespace MeetCampus.Contracts.Features.Auth;

public sealed record LoginResponse(
    bool Succeeded,
    bool RequiresTwoFactor,
    bool IsLockedOut,
    string? ErrorMessage);
