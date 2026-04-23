using System.Net.Http.Json;

namespace MeetCampus.Shared.Services.Http;

public abstract class BaseApiClient
{
    private readonly HttpClient _httpClient;

    protected BaseApiClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    protected async Task<TResponse> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = await SendCoreAsync(request, cancellationToken);
        return await ReadAsJsonAsync<TResponse>(response, cancellationToken);
    }

    protected async Task PutAsync<TRequest>(string requestUri, TRequest requestBody, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, requestUri)
        {
            Content = JsonContent.Create(requestBody),
        };

        using var response = await SendCoreAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendCoreAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw ApiClientException.FromNetwork(exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw ApiClientException.FromTimeout(exception);
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string? responseContent = null;
        if (response.Content is not null)
        {
            responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        }

        throw ApiClientException.FromStatusCode(response.StatusCode, responseContent);
    }

    private static async Task<TResponse> ReadAsJsonAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(response);

        await EnsureSuccessAsync(response, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        if (payload is null)
        {
            throw ApiClientException.FromEmptyResponse(response.StatusCode);
        }

        return payload;
    }
}
