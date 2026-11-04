namespace MeetCampus.Contracts.Features.Questionnaire;

public sealed record SubmitQuestionnaireResponseRequest(
    IReadOnlyList<QuestionnaireAnswerRequest> Answers);
