namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record RecoveryCodeLoginResponse(
    bool Succeeded,
    bool IsLockedOut,
    string? ErrorMessage);
