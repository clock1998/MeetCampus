namespace MeetCampus.Data.Matching;

public sealed class IntentCompatibilityRule
{
    public Guid Id { get; set; }
    public Guid SenderIntentId { get; set; }
    public UserIntention SenderIntent { get; set; } = default!;
    public Guid RecipientIntentId { get; set; }
    public UserIntention RecipientIntent { get; set; } = default!;
    public bool IsAllowed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
