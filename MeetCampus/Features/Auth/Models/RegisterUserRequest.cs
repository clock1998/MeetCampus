namespace MeetCampus.Features.Auth.Models;

internal sealed record RegisterUserRequest(
    string Email,
    string Password);
