namespace MeetCampus.Contracts.Features.Auth;

public sealed record AccountOperationResponse(
    bool Succeeded,
    IReadOnlyList<string>? Errors = null);
