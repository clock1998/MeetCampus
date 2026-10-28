namespace MeetCampus.Data.Matching;

public sealed class RecommendationItem
{
    public Guid Id { get; set; }
    public Guid RecommendationBatchId { get; set; }
    public RecommendationBatch RecommendationBatch { get; set; } = default!;
    public Guid CandidateUserId { get; set; }
    public int CompatibilityScore { get; set; }
    public decimal EvidenceCoverage { get; set; }
    public string CategoryScoresJson { get; set; } = "{}";
    public RecommendationDecision Decision { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
}
