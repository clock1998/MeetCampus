namespace MeetCampus.Features.Auth.Models;

public sealed record PasswordResetResult
{
    public bool UserNotFound { get; init; }

    public bool Succeeded { get; init; }

    public IReadOnlyList<string>? Errors { get; init; }

    public static PasswordResetResult NotFound() => new() { UserNotFound = true };

    public static PasswordResetResult Success() => new() { Succeeded = true };

    public static PasswordResetResult Failed(IEnumerable<string> errors) => new() { Errors = errors.ToArray() };
}
