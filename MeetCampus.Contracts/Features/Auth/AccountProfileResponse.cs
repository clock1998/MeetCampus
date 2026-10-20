namespace MeetCampus.Contracts.Features.Auth;

public sealed record AccountProfileResponse(
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool HasPassword);
