namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
