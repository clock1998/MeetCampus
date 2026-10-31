using System.ComponentModel.DataAnnotations;

namespace MeetCampus.Contracts.Features.Administration;

public sealed class QuestionRequest
{
    [Required, StringLength(128, MinimumLength = 1)]
    public string Code { get; set; } = string.Empty;

    public QuestionType Type { get; set; }

    [Required, StringLength(2000, MinimumLength = 1)]
    public string PromptEn { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 1)]
    public string PromptFr { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    [Required, StringLength(64, MinimumLength = 1)]
    public string CategoryCode { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "1")]
    public decimal ScoringWeight { get; set; }

    public int SortOrder { get; set; }

    public int? MaxSelections { get; set; }

    public int? ScaleMin { get; set; }

    public int? ScaleMax { get; set; }

    public List<QuestionOptionAdminRequest> Options { get; set; } = [];
}
