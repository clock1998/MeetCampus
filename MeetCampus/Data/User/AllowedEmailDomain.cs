namespace MeetCampus.Data.User;

public sealed class AllowedEmailDomain
{
    public Guid Id { get; set; }
    public string Domain { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}
