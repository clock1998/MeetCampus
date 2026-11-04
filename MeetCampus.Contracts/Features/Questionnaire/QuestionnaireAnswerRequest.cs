namespace MeetCampus.Contracts.Features.Questionnaire;

public sealed record QuestionnaireAnswerRequest(
    Guid QuestionId,
    IReadOnlyList<string> OptionCodes,
    int? ScaleValue);
