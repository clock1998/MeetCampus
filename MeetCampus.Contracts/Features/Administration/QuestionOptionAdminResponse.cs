namespace MeetCampus.Contracts.Features.Administration;

public sealed record QuestionOptionAdminResponse(
    string Code,
    string LabelEn,
    string LabelFr,
    int SortOrder,
    string? ScoringValue);
