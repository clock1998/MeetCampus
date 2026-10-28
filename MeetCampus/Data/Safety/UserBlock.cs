namespace MeetCampus.Data.Safety;

public sealed class UserBlock
{
    public Guid Id { get; set; }
    public Guid BlockerUserId { get; set; }
    public Guid BlockedUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
