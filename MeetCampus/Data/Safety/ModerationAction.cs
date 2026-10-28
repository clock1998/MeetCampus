namespace MeetCampus.Data.Safety;

public sealed class ModerationAction
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public UserReport Report { get; set; } = default!;
    public Guid ModeratorUserId { get; set; }
    public ModerationActionType Action { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
