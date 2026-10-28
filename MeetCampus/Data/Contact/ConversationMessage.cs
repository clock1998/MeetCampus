using MeetCampus.Data.User;

namespace MeetCampus.Data.Contact;

public sealed class ConversationMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = default!;
    public Guid SenderUserId { get; set; }
    public ApplicationUser SenderUser { get; set; } = default!;
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
