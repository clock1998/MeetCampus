using System.ComponentModel.DataAnnotations;

namespace MeetCampus.Contracts.Features.Profile.Contracts;

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

    [Required(ErrorMessage = "ProfileSetup_Required_Intention")]
    public Guid? IntentionId { get; set; }

    [Required(ErrorMessage = "ProfileSetup_Required_Ethnicity")]
    public Guid? EthnicityId { get; set; }
}
