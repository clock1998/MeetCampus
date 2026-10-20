namespace MeetCampus.Contracts.Features.Auth;

public sealed record ResetPasswordResponse(
    bool Succeeded,
    IReadOnlyList<string>? Errors);
