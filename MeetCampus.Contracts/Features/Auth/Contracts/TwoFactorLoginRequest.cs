namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record TwoFactorLoginRequest(
    string Code,
    bool RememberMe,
    bool RememberMachine);
