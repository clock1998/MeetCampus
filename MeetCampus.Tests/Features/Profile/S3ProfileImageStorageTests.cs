using Amazon.S3;
using MeetCampus.Features.Profile.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace MeetCampus.Tests.Features.Profile;

public sealed class S3ProfileImageStorageTests
{
    [Fact]
    public void WhenCreatingReadUrlWithValidConfigurationThenUrlUsesConfiguredEndpointAndBucket()
    {
        using var storage = new S3ProfileImageStorage(Options.Create(new S3Options
        {
            Endpoint = "https://garage.example.test:3900/",
            BucketName = "profile-images",
            AccessId = "test-access-key",
            AccessSecret = "test-secret-key"
        }));

        var url = storage.GetReadUrl("users/test/profile.png");

        Assert.StartsWith("https://garage.example.test:3900/profile-images/users/test/profile.png?", url, StringComparison.Ordinal);
    }

    [Fact]
    public void WhenEndpointIsNotAnAbsoluteHttpUrlThenCreatingReadUrlFails()
    {
        using var storage = new S3ProfileImageStorage(Options.Create(new S3Options
        {
            Endpoint = "file:///tmp/garage",
            BucketName = "profile-images",
            AccessId = "test-access-key",
            AccessSecret = "test-secret-key"
        }));

        Assert.Throws<InvalidOperationException>(() => storage.GetReadUrl("users/test/profile.png"));
    }
}
