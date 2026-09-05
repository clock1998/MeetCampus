using System.Net.Http.Json;

namespace MeetCampus.HttpClients.Infrastructure.Http;

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

    protected Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default) =>
        SendCoreAsync(request, cancellationToken);

    protected async Task PutAsync<TRequest>(string requestUri, TRequest requestBody, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, requestUri)
        {
            Content = JsonContent.Create(requestBody),
        };

        using var response = await SendCoreAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest requestBody, CancellationToken cancellationToken = default)
    {
        return await PostAsync<TRequest, TResponse>(requestUri, requestBody, null, cancellationToken);
    }

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(
        string requestUri,
        TRequest requestBody,
        Action<HttpRequestMessage>? configureRequest,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(requestBody),
        };
        configureRequest?.Invoke(request);

        using var response = await SendCoreAsync(request, cancellationToken);
        return await ReadAsJsonAsync<TResponse>(response, cancellationToken);
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
