namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record RecoveryCodeLoginRequest(
    string RecoveryCode);
