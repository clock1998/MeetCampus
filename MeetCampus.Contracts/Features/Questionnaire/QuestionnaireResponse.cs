namespace MeetCampus.Contracts.Features.Questionnaire;

public sealed record QuestionnaireResponse(
    IReadOnlyList<QuestionnaireQuestionResponse> Questions,
    bool IsComplete);
