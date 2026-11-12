namespace MeetCampus.Features.Matching.Models;

public sealed record CompatibilityScoreResult(decimal? Score, decimal EvidenceCoverage, IReadOnlyList<CategoryScore> Categories);
