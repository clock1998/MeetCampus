namespace MeetCampus.Contracts.Features.Auth;

public sealed record RecoveryCodeLoginResponse(
    bool Succeeded,
    bool IsLockedOut,
    string? ErrorMessage);
