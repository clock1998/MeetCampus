using MeetCampus.Contracts.Features.Administration;

namespace MeetCampus.Contracts.Features.Questionnaire;

public sealed record QuestionnaireQuestionResponse(
    Guid Id,
    string Code,
    QuestionType Type,
    string Prompt,
    string CategoryCode,
    int? ScaleMin,
    int? ScaleMax,
    IReadOnlyList<QuestionnaireOptionResponse> Options,
    IReadOnlyList<string> SelectedOptionCodes,
    int? ScaleValue,
    bool IsRefreshRequired);
