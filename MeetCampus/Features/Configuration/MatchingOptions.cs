using System.ComponentModel.DataAnnotations;

namespace MeetCampus.Features.Configuration;

public sealed class MatchingOptions : IValidatableObject
{
    public int NewRecommendationsPerUserPerDay { get; set; } = 3;
    public int MaxUnseenRecommendations { get; set; } = 9;
    public string RefreshPeriod { get; set; } = "daily";
    public int PassedProfileSuppressionDays { get; set; } = 90;
    public int MinimumCompatibilityScore { get; set; } = 30;
    public int AlgorithmVersion { get; set; } = 1;
    public int ScoringConfigurationVersion { get; set; } = 1;
    public bool UsePopularitySignals { get; set; }
    public bool DisplayExactScore { get; set; }
    public MatchingWeightsOptions Weights { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NewRecommendationsPerUserPerDay is < 1 or > 100 || MaxUnseenRecommendations is < 1 or > 1000 || NewRecommendationsPerUserPerDay > MaxUnseenRecommendations)
            yield return new ValidationResult("Recommendation limits must be within supported quotas, and daily recommendations cannot exceed the unseen limit.");

        if (!string.Equals(RefreshPeriod, "daily", StringComparison.OrdinalIgnoreCase))
            yield return new ValidationResult("The MVP recommendation refresh period must be daily.");

        if (PassedProfileSuppressionDays is < 1 or > 3650)
            yield return new ValidationResult("Passed-profile suppression must be between 1 and 3650 days.");

        if (MinimumCompatibilityScore is < 0 or > 100)
            yield return new ValidationResult("Minimum compatibility score must be between 0 and 100.");

        if (AlgorithmVersion <= 0 || ScoringConfigurationVersion <= 0)
            yield return new ValidationResult("Algorithm and scoring configuration versions must be positive.");

        if (UsePopularitySignals)
            yield return new ValidationResult("Popularity signals are not permitted in the MVP.");

        if (DisplayExactScore)
            yield return new ValidationResult("Exact compatibility scores must not be displayed.");

        if (Weights is null || !Weights.IsValid())
            yield return new ValidationResult("Matching category weights must be nonnegative and sum to 1.00.");
    }
}
