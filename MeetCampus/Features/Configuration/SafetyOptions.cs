using System.ComponentModel.DataAnnotations;

namespace MeetCampus.Features.Configuration;

public sealed class SafetyOptions : IValidatableObject
{
    public bool BlockingIsImmediate { get; set; } = true;
    public bool ReportingRequiredForMvp { get; set; } = true;
    public bool ModerationActionsAudited { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!BlockingIsImmediate || !ReportingRequiredForMvp || !ModerationActionsAudited)
            yield return new ValidationResult("MVP safety requires immediate blocking, reporting, and audited moderation actions.");
    }
}
