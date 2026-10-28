using System.ComponentModel.DataAnnotations;

namespace MeetCampus.Contracts.Features.Profile;

public sealed record UserProfileSetupForm
{
    public Guid? GenderId { get; set; }

    public Guid? SexualityId { get; set; }

    [Required(ErrorMessage = "ProfileSetup_Required_StudyDomain")]
    public Guid? StudyDomainId { get; set; }

    [Required(ErrorMessage = "ProfileSetup_Required_School")]
    public Guid? SchoolId { get; set; }

    [Required(ErrorMessage = "ProfileSetup_Required_Language")]
    public Guid? LanguageId { get; set; }

    [MinLength(1, ErrorMessage = "ProfileSetup_Required_Intention")]
    public List<Guid> IntentionIds { get; set; } = [];

    [Required(ErrorMessage = "ProfileSetup_Required_Ethnicity")]
    public Guid? EthnicityId { get; set; }
}
