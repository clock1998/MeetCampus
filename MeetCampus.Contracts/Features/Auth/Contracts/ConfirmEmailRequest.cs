namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record ConfirmEmailRequest(
    string UserId,
    string Code,
    string? Email);
