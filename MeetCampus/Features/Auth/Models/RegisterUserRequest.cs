namespace MeetCampus.Features.Auth.Models;

public sealed record RegisterUserRequest(
    string Email,
    string Password);
