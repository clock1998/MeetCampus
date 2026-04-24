namespace MeetCampus.Shared.Models;

public sealed record ProfileLookupOption(Guid Id, string? DisplayKey, string Name);

public sealed record UserProfileSetupForm
{
    public Guid? GenderId { get; set; }

    public Guid? SexualityId { get; set; }

    public Guid? StudyDomainId { get; set; }

    public Guid? SchoolId { get; set; }

    public Guid? LanguageId { get; set; }

    public Guid? IntentionId { get; set; }

    public Guid? EthnicityId { get; set; }
}

public sealed record UserProfileSetupResponse
{
    public UserProfileSetupForm Profile { get; set; } = new();

    public IReadOnlyList<ProfileLookupOption> Genders { get; set; } = [];

    public IReadOnlyList<ProfileLookupOption> Sexualities { get; set; } = [];

    public IReadOnlyList<ProfileLookupOption> StudyDomains { get; set; } = [];

    public IReadOnlyList<ProfileLookupOption> Schools { get; set; } = [];

    public IReadOnlyList<ProfileLookupOption> Languages { get; set; } = [];

    public IReadOnlyList<ProfileLookupOption> Intentions { get; set; } = [];

    public IReadOnlyList<ProfileLookupOption> Ethnicities { get; set; } = [];
}

public sealed record UpdateUserProfileRequest
{
    public Guid? GenderId { get; set; }

    public Guid? SexualityId { get; set; }

    public Guid StudyDomainId { get; set; }

    public Guid SchoolId { get; set; }

    public Guid LanguageId { get; set; }

    public Guid IntentionId { get; set; }

    public Guid EthnicityId { get; set; }
}
