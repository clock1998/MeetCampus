using MeetCampus.Contracts.Features.Administration;

namespace MeetCampus.Data.Questionnaire;

public sealed class Question
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public bool IsRequired { get; set; }
    public string Group { get; set; } = string.Empty;
    public decimal ScoringWeight { get; set; }
    public int SortOrder { get; set; } = 0;

    // Relevant for MultipleChoice.
    public int? MaxSelections { get; set; }

    // Relevant for Scale.
    public int? ScaleMin { get; set; }

    public int? ScaleMax { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<QuestionOption> Options { get; set; } = [];
    public ICollection<QuestionTranslation> Translations { get; set; } = [];
}