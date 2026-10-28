namespace MeetCampus.Data.Profiles;

public sealed class UserProfileIntention
{
    public Guid UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = default!;
    public Guid UserIntentionId { get; set; }
    public UserIntention UserIntention { get; set; } = default!;
}
