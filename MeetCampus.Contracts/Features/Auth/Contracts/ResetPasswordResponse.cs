namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record ResetPasswordResponse(
    bool Succeeded,
    IReadOnlyList<string>? Errors);
