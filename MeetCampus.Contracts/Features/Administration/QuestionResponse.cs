namespace MeetCampus.Contracts.Features.Administration;

public sealed record QuestionResponse(
    Guid Id,
    string Code,
    QuestionType Type,
    string PromptEn,
    string PromptFr,
    Guid CategoryId,
    decimal ScoringWeight,
    int SortOrder,
    int? MaxSelections,
    int? ScaleMin,
    int? ScaleMax,
    IReadOnlyList<QuestionOptionAdminResponse> Options,
    DateTimeOffset UpdatedAt);
