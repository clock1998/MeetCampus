namespace MeetCampus.Contracts.Features.Auth;

public sealed record AuthUserResponse(
    bool IsAuthenticated,
    string? UserId,
    string? Email,
    IReadOnlyList<string> Roles);
