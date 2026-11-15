namespace MeetCampus.Features.Matching.Models;

public sealed record MatchExplanationResult(
    MatchLabel? Label,
    bool IsComplete,
    IReadOnlyList<MatchExplanationCategory> Categories);
