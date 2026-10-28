using System.ComponentModel.DataAnnotations;

namespace MeetCampus.Features.Configuration;

public sealed class VerificationOptions : IValidatableObject
{
    public int TokenLifetimeMinutes { get; set; } = 30;
    public int MaxRequestsPerHourPerAddress { get; set; } = 5;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TokenLifetimeMinutes is < 5 or > 1440)
            yield return new ValidationResult("Verification token lifetime must be between 5 minutes and 24 hours.");

        if (MaxRequestsPerHourPerAddress is < 1 or > 20)
            yield return new ValidationResult("Verification requests per hour per address must be between 1 and 20.");
    }
}
