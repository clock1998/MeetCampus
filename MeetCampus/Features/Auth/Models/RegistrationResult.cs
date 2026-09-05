using Microsoft.AspNetCore.Identity;

namespace MeetCampus.Features.Auth.Models;

public sealed record RegistrationResult
{
    public bool Succeeded { get; init; }

    public bool RequiresConfirmedAccount { get; init; }

    public IEnumerable<IdentityError>? Errors { get; init; }

    public static RegistrationResult Success(bool requiresConfirmedAccount) => new()
    {
        Succeeded = true,
        RequiresConfirmedAccount = requiresConfirmedAccount,
    };

    public static RegistrationResult Failed(IEnumerable<IdentityError> errors) => new()
    {
        Errors = errors,
    };
}
