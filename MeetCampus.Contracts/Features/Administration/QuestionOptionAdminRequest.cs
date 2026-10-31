using System.ComponentModel.DataAnnotations;

namespace MeetCampus.Contracts.Features.Administration;

public sealed class QuestionOptionAdminRequest
{
    [Required, StringLength(128, MinimumLength = 1)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(512, MinimumLength = 1)]
    public string LabelEn { get; set; } = string.Empty;

    [Required, StringLength(512, MinimumLength = 1)]
    public string LabelFr { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    [StringLength(128)]
    public string? ScoringValue { get; set; }
}
