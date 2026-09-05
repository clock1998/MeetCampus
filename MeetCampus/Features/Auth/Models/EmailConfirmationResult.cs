namespace MeetCampus.Features.Auth.Models;

public sealed record EmailConfirmationResult
{
    public bool UserNotFound { get; init; }

    public bool Succeeded { get; init; }

    public static EmailConfirmationResult NotFound() => new() { UserNotFound = true };

    public static EmailConfirmationResult Success() => new() { Succeeded = true };

    public static EmailConfirmationResult Failed() => new();
}
