namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record TwoFactorLoginResponse(
    bool Succeeded,
    bool IsLockedOut,
    string? ErrorMessage);
