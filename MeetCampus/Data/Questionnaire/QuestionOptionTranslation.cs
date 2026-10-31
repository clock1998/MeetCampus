namespace MeetCampus.Data.Questionnaire;

public sealed class QuestionOptionTranslation
{
    public Guid QuestionOptionId { get; set; }
    public QuestionOption QuestionOption { get; set; } = default!;
    public string CultureCode { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
