namespace MeetCampus.Data.Questionnaire;

public sealed class QuestionTranslation
{
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = default!;
    public string CultureCode { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
