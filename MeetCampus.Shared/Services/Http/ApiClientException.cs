using System.Net;

namespace MeetCampus.Shared.Services.Http;

public sealed class ApiClientException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    public string? ResponseContent { get; }
    public bool IsNetworkError { get; }

    private ApiClientException(
        string message,
        Exception? innerException = null,
        HttpStatusCode? statusCode = null,
        string? responseContent = null,
        bool isNetworkError = false)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
        IsNetworkError = isNetworkError;
    }

    public static ApiClientException FromNetwork(HttpRequestException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return new ApiClientException("A network error occurred while calling the API.", exception, isNetworkError: true);
    }

    public static ApiClientException FromTimeout(TaskCanceledException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return new ApiClientException("The API request timed out.", exception, isNetworkError: true);
    }

    public static ApiClientException FromStatusCode(HttpStatusCode statusCode, string? responseContent)
    {
        return new ApiClientException(
            $"The API returned an unsuccessful status code: {(int)statusCode} ({statusCode}).",
            statusCode: statusCode,
            responseContent: responseContent);
    }

    public static ApiClientException FromEmptyResponse(HttpStatusCode statusCode)
    {
        return new ApiClientException(
            "The API returned an empty response body.",
            statusCode: statusCode);
    }
}
