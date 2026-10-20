namespace MeetCampus.Contracts.Features.Auth;

public sealed record TwoFactorLoginRequest(
    string Code,
    bool RememberMe,
    bool RememberMachine);
