using MeetCampus.Data.User;

namespace MeetCampus.Data.Questionnaire;

public sealed class UserAnswer
{
    public Guid Id { get; set; }
    public Guid ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = default!;
    public Guid QuestionOptionId { get; set; }
    public QuestionOption QuestionOption { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
}
