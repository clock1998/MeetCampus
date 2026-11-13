namespace MeetCampus.Data.Questionnaire;

public sealed class QuestionCategory
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameFr { get; set; } = string.Empty;
    public decimal MatchingWeight { get; set; }
    public int SortOrder { get; set; }
    public ICollection<Question> Questions { get; set; } = [];
}
