using System.Net.Http.Json;
using MeetCampus.Client.Shared;
using MeetCampus.Contracts.Features.Administration;

namespace MeetCampus.Client.Features.Administration.HttpClients;

public sealed class QuestionnaireAdminClient(HttpClient httpClient) : BaseApiClient(httpClient), IQuestionnaireAdminClient
{
    private const string Endpoint = "/api/admin/questionnaire";

    public Task<IReadOnlyList<QuestionResponse>> GetQuestionsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<QuestionResponse>>(Endpoint, cancellationToken);

    public Task<QuestionCatalogResponse> GetCatalogAsync(CancellationToken cancellationToken = default) =>
        GetAsync<QuestionCatalogResponse>($"{Endpoint}/catalog", cancellationToken);

    public async Task CreateQuestionAsync(QuestionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, Endpoint) { Content = JsonContent.Create(request) };
        using var response = await SendAsync(httpRequest, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateQuestionAsync(Guid questionId, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"{Endpoint}/{questionId:D}") { Content = JsonContent.Create(request) };
        using var response = await SendAsync(httpRequest, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteQuestionAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{Endpoint}/{questionId:D}");
        using var response = await SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }
}
