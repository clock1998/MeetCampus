namespace MeetCampus.Contracts.Features.Auth;

public sealed record RegisterResponse(
    bool Succeeded,
    bool RequiresConfirmedAccount,
    IReadOnlyList<string>? Errors);
