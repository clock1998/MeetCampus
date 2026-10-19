using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace MeetCampus.Features.Profile.Services;

public sealed class S3ProfileImageStorage(IOptions<S3Options> options) : IDisposable
{
    private static readonly TimeSpan ReadUrlLifetime = TimeSpan.FromMinutes(30);
    private readonly S3Options _options = options.Value;
    private readonly object _clientLock = new();
    private IAmazonS3? _client;

    public async Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        await Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
        }, cancellationToken);
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        await Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = BucketName,
            Key = key,
        }, cancellationToken);
    }

    public string GetReadUrl(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = BucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(ReadUrlLifetime),
            Protocol = Protocol.HTTPS,
        });
    }

    private string BucketName => GetRequiredSetting(_options.BucketName, nameof(_options.BucketName));

    private IAmazonS3 Client
    {
        get
        {
            lock (_clientLock)
            {
                if (_client is not null)
                    return _client;

                var endpoint = GetRequiredSetting(_options.Endpoint, nameof(_options.Endpoint));
                var accessId = GetRequiredSetting(_options.AccessId, nameof(_options.AccessId));
                var accessSecret = GetRequiredSetting(_options.AccessSecret, nameof(_options.AccessSecret));

                if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var serviceUri)
                    || serviceUri.Scheme is not ("http" or "https"))
                {
                    throw new InvalidOperationException("The configured S3 endpoint must be an absolute HTTP or HTTPS URL.");
                }

                _client = new AmazonS3Client(
                    new BasicAWSCredentials(accessId, accessSecret),
                    new AmazonS3Config
                    {
                        ServiceURL = endpoint,
                        ForcePathStyle = true,
                    });

                return _client;
            }
        }
    }

    private static string GetRequiredSetting(string value, string settingName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"S3 setting '{settingName}' is required.");

        return value;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
