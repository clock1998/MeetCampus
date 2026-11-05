using Microsoft.AspNetCore.WebUtilities;

namespace MeetCampus.Shared;

public static class ClientUrlBuilder
{
    public static string Build(string scheme, HostString host, string path, IReadOnlyDictionary<string, object?> parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scheme);
        ArgumentNullException.ThrowIfNull(parameters);

        var baseUri = $"{scheme}://{host}{path}";
        return QueryHelpers.AddQueryString(
            baseUri,
            parameters.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString()));
    }
}
