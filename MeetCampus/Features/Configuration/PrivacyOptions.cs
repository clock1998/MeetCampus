using System.ComponentModel.DataAnnotations;

namespace MeetCampus.Features.Configuration;

public sealed class PrivacyOptions : IValidatableObject
{
    public bool UniversityEmailPublic { get; set; }
    public bool PrivateAnswersInPublicProfile { get; set; }
    public bool PrivateAnswersInExplanations { get; set; }
    public bool DiscoverabilityRequiresUserOptIn { get; set; } = true;
    public bool ProfileImageRequiredForDiscoverability { get; set; } = true;
    public bool ProfileImageApprovalRequiredForDiscoverability { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UniversityEmailPublic)
            yield return new ValidationResult("University email addresses must not be public.");

        if (PrivateAnswersInPublicProfile || PrivateAnswersInExplanations)
            yield return new ValidationResult("Private questionnaire answers must not be exposed in profiles or explanations.");

        if (!DiscoverabilityRequiresUserOptIn || !ProfileImageRequiredForDiscoverability)
            yield return new ValidationResult("Discoverability requires explicit opt-in and at least one uploaded profile image.");

        if (ProfileImageApprovalRequiredForDiscoverability)
            yield return new ValidationResult("Image approval is deferred and must not be required for MVP discoverability.");
    }
}
