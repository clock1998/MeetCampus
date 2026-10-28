namespace MeetCampus.Data.Safety;

public sealed class UserReport
{
    public Guid Id { get; set; }
    public Guid ReporterUserId { get; set; }
    public Guid ReportedUserId { get; set; }
    public UserReportReason Reason { get; set; }
    public string? Description { get; set; }
    public UserReportStatus Status { get; set; } = UserReportStatus.Open;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}
