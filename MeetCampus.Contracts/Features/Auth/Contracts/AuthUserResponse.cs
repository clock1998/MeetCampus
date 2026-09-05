namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record AuthUserResponse(
    bool IsAuthenticated,
    string? UserId,
    string? Email,
    IReadOnlyList<string> Roles);
