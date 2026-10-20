namespace MeetCampus.Contracts.Features.Auth;

public sealed record TwoFactorLoginResponse(
    bool Succeeded,
    bool IsLockedOut,
    string? ErrorMessage);
