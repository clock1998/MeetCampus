namespace MeetCampus.Features.Configuration;

public sealed class MatchingWeightsOptions
{
    public decimal Interests { get; set; } = 0.25m;
    public decimal Values { get; set; } = 0.30m;
    public decimal SocialCommunication { get; set; } = 0.20m;
    public decimal LifestyleAvailability { get; set; } = 0.15m;
    public decimal ConnectionIntentExpectations { get; set; } = 0.10m;

    internal bool IsValid() =>
        Interests >= 0 &&
        Values >= 0 &&
        SocialCommunication >= 0 &&
        LifestyleAvailability >= 0 &&
        ConnectionIntentExpectations >= 0 &&
        Interests + Values + SocialCommunication + LifestyleAvailability + ConnectionIntentExpectations == 1.00m;
}
