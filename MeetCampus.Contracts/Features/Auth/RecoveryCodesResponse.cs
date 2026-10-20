namespace MeetCampus.Contracts.Features.Auth;

public sealed record RecoveryCodesResponse(
    IReadOnlyList<string> Codes);
