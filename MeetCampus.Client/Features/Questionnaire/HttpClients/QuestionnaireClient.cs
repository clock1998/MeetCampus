using System.Net.Http.Json;
using MeetCampus.Client.Shared;
using MeetCampus.Contracts.Features.Questionnaire;

namespace MeetCampus.Client.Features.Questionnaire.HttpClients;

public sealed class QuestionnaireClient(HttpClient httpClient) : BaseApiClient(httpClient), IQuestionnaireClient
{
    private const string Endpoint = "/api/questionnaire";

    public Task<QuestionnaireResponse> GetAsync(string cultureCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureCode);
        return GetAsync<QuestionnaireResponse>($"{Endpoint}?culture={Uri.EscapeDataString(cultureCode)}", cancellationToken);
    }

    public async Task SubmitAsync(SubmitQuestionnaireResponseRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, Endpoint) { Content = JsonContent.Create(request) };
        using var response = await SendAsync(httpRequest, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }
}
