namespace MeetCampus.Contracts.Features.Auth;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
