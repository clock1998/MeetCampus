namespace MeetCampus.Data.Questionnaire;

public sealed class QuestionOption
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = default!;
    public string OptionCode { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? ScoringValue { get; set; }
    public ICollection<QuestionOptionTranslation> Translations { get; set; } = [];
}
