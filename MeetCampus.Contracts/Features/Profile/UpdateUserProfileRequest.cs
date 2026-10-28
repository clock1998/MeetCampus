namespace MeetCampus.Contracts.Features.Profile;

public sealed record UpdateUserProfileRequest
{
    public Guid? GenderId { get; set; }

    public Guid? SexualityId { get; set; }

    public Guid StudyDomainId { get; set; }

    public Guid SchoolId { get; set; }

    public Guid LanguageId { get; set; }

    public List<Guid> IntentionIds { get; set; } = [];

    public Guid EthnicityId { get; set; }
}
