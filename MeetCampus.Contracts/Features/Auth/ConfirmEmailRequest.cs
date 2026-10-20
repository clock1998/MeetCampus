namespace MeetCampus.Contracts.Features.Auth;

public sealed record ConfirmEmailRequest(
    string UserId,
    string Code,
    string? Email);
