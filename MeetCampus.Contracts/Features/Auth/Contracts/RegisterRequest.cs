namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string? ReturnUrl);
