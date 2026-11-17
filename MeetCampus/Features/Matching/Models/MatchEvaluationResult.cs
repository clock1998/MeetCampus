namespace MeetCampus.Features.Matching.Models;

public sealed record MatchEvaluationResult(
    MatchEvaluationStatus Status,
    CompatibilityScoreResult? Score,
    MatchExplanationResult? Explanation);
