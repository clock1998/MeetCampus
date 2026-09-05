namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record RegisterResponse(
    bool Succeeded,
    bool RequiresConfirmedAccount,
    IReadOnlyList<string>? Errors);
