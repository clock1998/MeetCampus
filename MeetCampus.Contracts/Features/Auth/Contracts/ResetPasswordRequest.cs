namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword);
