using System.ComponentModel.DataAnnotations;

namespace MeetCampus.Features.Configuration;

public sealed class ContactOptions : IValidatableObject
{
    public int MaxPendingOutbound { get; set; } = 3;
    public int MaxInboundPerRolling7Days { get; set; } = 5;
    public int PendingExpiryDays { get; set; } = 7;
    public bool RequireMutualInterestForMessaging { get; set; } = true;
    public bool RemoveRecipientFromDiscoverabilityAtCapacity { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MaxPendingOutbound <= 0 || MaxPendingOutbound > 100)
            yield return new ValidationResult("Maximum pending outbound contacts must be between 1 and 100.");

        if (MaxInboundPerRolling7Days <= 0 || MaxInboundPerRolling7Days > 100)
            yield return new ValidationResult("Maximum inbound contacts per rolling 7 days must be between 1 and 100.");

        if (PendingExpiryDays is < 1 or > 365)
            yield return new ValidationResult("Pending contact expiry must be between 1 and 365 days.");

        if (!RequireMutualInterestForMessaging)
            yield return new ValidationResult("The MVP requires mutual interest before messaging.");

        if (!RemoveRecipientFromDiscoverabilityAtCapacity)
            yield return new ValidationResult("Recipients at capacity must be removed from discoverability.");
    }
}
