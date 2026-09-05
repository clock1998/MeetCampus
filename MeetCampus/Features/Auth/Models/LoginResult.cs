namespace MeetCampus.Features.Auth.Models;

internal sealed record LoginResult
{
    public bool Succeeded { get; init; }

    public bool RequiresTwoFactor { get; init; }

    public bool IsLockedOut { get; init; }

    public string? ErrorMessage { get; init; }

    public static LoginResult Success() => new() { Succeeded = true };

    public static LoginResult TwoFactorRequired() => new() { RequiresTwoFactor = true };

    public static LoginResult LockedOut() => new() { IsLockedOut = true };

    public static LoginResult Failed(string errorMessage) => new() { ErrorMessage = errorMessage };
}
