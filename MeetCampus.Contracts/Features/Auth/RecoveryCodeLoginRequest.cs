namespace MeetCampus.Contracts.Features.Auth;

public sealed record RecoveryCodeLoginRequest(
    string RecoveryCode);
