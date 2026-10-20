namespace MeetCampus.Contracts.Features.Auth;

public sealed record TwoFactorStatusResponse(
    bool CanTrack,
    bool IsEnabled,
    bool HasAuthenticator,
    bool IsMachineRemembered,
    int RecoveryCodesLeft);
