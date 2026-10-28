namespace MeetCampus.Data.Questionnaire;

public sealed class Question
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string PromptResourceKey { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string Group { get; set; } = string.Empty;
    public QuestionVisibility QuestionVisibility { get; set; }
    public decimal ScoringWeight { get; set; }
    public string? ExplanationTemplateKey { get; set; }
    public int SortOrder { get; set; } = 0;

    // Relevant for MultipleChoice.
    public int? MaxSelections { get; set; }

    // Relevant for Scale.
    public int? ScaleMin { get; set; }

    public int? ScaleMax { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<QuestionOption> Options { get; set; } = [];
}
public enum QuestionType
{
    SingleChoice = 1,
    MultipleChoice = 2,
    Scale = 3
}