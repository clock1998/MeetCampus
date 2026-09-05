namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record TwoFactorStatusResponse(
    bool CanTrack,
    bool IsEnabled,
    bool HasAuthenticator,
    bool IsMachineRemembered,
    int RecoveryCodesLeft);
