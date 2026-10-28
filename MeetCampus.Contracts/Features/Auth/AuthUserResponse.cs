namespace MeetCampus.Contracts.Features.Auth;

public sealed record AuthUserResponse(
    bool IsAuthenticated,
    Guid? UserId,
    string? Email,
    IReadOnlyList<string> Roles);
