using MeetCampus.Data.User;

namespace MeetCampus.Data.Contact;

public sealed class Conversation
{
    public Guid Id { get; set; }
    public Guid FirstUserId { get; set; }
    public ApplicationUser FirstUser { get; set; } = default!;
    public Guid SecondUserId { get; set; }
    public ApplicationUser SecondUser { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public ICollection<ConversationMessage> Messages { get; set; } = [];
}
