namespace MeetCampus.Data.User;

public sealed class AccountLifecycle
{
    public Guid ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = default!;
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public bool DiscoverabilityOptIn { get; set; }
    public DateTimeOffset? EmailVerifiedAt { get; set; }
    public DateTimeOffset? DeactivatedAt { get; set; }
    public DateTimeOffset? RemovalRequestedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }
    public DateTimeOffset? SuspendedUntil { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
