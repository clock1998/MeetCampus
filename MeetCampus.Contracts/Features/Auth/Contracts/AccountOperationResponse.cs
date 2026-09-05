namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record AccountOperationResponse(
    bool Succeeded,
    IReadOnlyList<string>? Errors = null);
