namespace MeetCampus.Contracts.Features.Auth;

public sealed record ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword);
