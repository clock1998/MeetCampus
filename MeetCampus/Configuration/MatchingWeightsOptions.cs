namespace MeetCampus.Configuration;

public sealed class MatchingWeightsOptions
{
    public decimal Interests { get; set; } = 0.25m;
    public decimal Values { get; set; } = 0.30m;
    public decimal SocialCommunication { get; set; } = 0.20m;
    public decimal LifestyleAvailability { get; set; } = 0.15m;
    public decimal ConnectionIntentExpectations { get; set; } = 0.10m;

    /// <summary>Returns the configured weight for a supported questionnaire category.</summary>
    public decimal GetCategoryWeight(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("A category is required.", nameof(category));
        if (!IsValid())
            throw new InvalidOperationException("Matching category weights must be nonnegative and sum to 1.00.");

        return category switch
        {
            "interests" => Interests,
            "values" => Values,
            "social" => SocialCommunication,
            "lifestyle" => LifestyleAvailability,
            "connection-intent" => ConnectionIntentExpectations,
            _ => throw new ArgumentException("The category is not supported.", nameof(category))
        };
    }

    internal bool IsValid() =>
        Interests >= 0 &&
        Values >= 0 &&
        SocialCommunication >= 0 &&
        LifestyleAvailability >= 0 &&
        ConnectionIntentExpectations >= 0 &&
        Interests + Values + SocialCommunication + LifestyleAvailability + ConnectionIntentExpectations == 1.00m;
}
