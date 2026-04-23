namespace MeetCampus.Data;

public sealed class UserProfile
{
    public Guid Id { get; set; }

    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = default!;

    public string? Gender { get; set; }
    public string? Sexuality { get; set; }

    public Guid StudyDomainId { get; set; }
    public StudyDomain StudyDomain { get; set; } = default!;

    public Guid SchoolId { get; set; }
    public School School { get; set; } = default!;

    public Guid LanguageId { get; set; }
    public Language Language { get; set; } = default!;

    public Guid IntentionId { get; set; }
    public UserIntention Intention { get; set; } = default!;

    public Guid EthnicityId { get; set; }
    public UserEthnicity Ethnicity { get; set; } = default!;
}
