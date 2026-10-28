using MeetCampus.Data.User;

namespace MeetCampus.Data.Audit;

public sealed class AuditEvent
{
    public Guid Id { get; set; }
    public Guid ActorUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = default!;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public string? DetailsJson { get; set; }
}
