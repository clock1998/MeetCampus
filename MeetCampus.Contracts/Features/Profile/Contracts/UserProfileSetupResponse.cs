namespace MeetCampus.Contracts.Features.Profile.Contracts;

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
