using MeetCampus.Contracts.Features.Questionnaire;

namespace MeetCampus.Client.Features.Questionnaire.HttpClients;

public interface IQuestionnaireClient
{
    Task<QuestionnaireResponse> GetAsync(string cultureCode, CancellationToken cancellationToken = default);

    Task SubmitAsync(SubmitQuestionnaireResponseRequest request, CancellationToken cancellationToken = default);
}
