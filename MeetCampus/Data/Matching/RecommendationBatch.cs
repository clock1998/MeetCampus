using MeetCampus.Data.User;

namespace MeetCampus.Data.Matching;

public sealed class RecommendationBatch
{
    public Guid Id { get; set; }
    public Guid ViewerUserId { get; set; }
    public ApplicationUser ViewerUser { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateOnly BatchDate { get; set; }
    public string AlgorithmVersion { get; set; } = string.Empty;
    public Guid? ScoringConfigurationVersionId { get; set; }
    public string ScoringWeightsJson { get; set; } = "{}";
    public ICollection<RecommendationItem> Items { get; set; } = [];
}
