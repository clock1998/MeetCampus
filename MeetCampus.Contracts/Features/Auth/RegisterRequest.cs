namespace MeetCampus.Contracts.Features.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string? ReturnUrl);
