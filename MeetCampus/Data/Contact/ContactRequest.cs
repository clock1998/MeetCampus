using MeetCampus.Data.User;

namespace MeetCampus.Data.Contact;

public sealed class ContactRequest
{
    public Guid Id { get; set; }
    public Guid SenderUserId { get; set; }
    public Guid RecipientUserId { get; set; }
    public Guid SenderIntentId { get; set; }
    public ApplicationUser Sender { get; set; } = default!;
    public Guid RecipientIntentId { get; set; }
    public ApplicationUser Recipient { get; set; } = default!;
    public ContactState State { get; set; } = ContactState.Pending;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? ConversationId { get; set; }
    public Conversation? Conversation { get; set; }
}
