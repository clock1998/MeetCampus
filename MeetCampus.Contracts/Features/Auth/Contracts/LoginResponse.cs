namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record LoginResponse(
    bool Succeeded,
    bool RequiresTwoFactor,
    bool IsLockedOut,
    string? ErrorMessage);
