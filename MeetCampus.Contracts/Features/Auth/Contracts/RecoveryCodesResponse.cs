namespace MeetCampus.Contracts.Features.Auth.Contracts;

public sealed record RecoveryCodesResponse(
    IReadOnlyList<string> Codes);
