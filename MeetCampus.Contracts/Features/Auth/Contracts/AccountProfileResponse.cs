namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record AccountProfileResponse(
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool HasPassword);
