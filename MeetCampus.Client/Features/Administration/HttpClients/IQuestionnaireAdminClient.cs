using MeetCampus.Contracts.Features.Administration;

namespace MeetCampus.Client.Features.Administration.HttpClients;

public interface IQuestionnaireAdminClient
{
    Task<IReadOnlyList<QuestionResponse>> GetQuestionsAsync(CancellationToken cancellationToken = default);

    Task<QuestionCatalogResponse> GetCatalogAsync(CancellationToken cancellationToken = default);

    Task CreateQuestionAsync(QuestionRequest request, CancellationToken cancellationToken = default);

    Task UpdateQuestionAsync(Guid questionId, QuestionRequest request, CancellationToken cancellationToken = default);

    Task DeleteQuestionAsync(Guid questionId, CancellationToken cancellationToken = default);
}
