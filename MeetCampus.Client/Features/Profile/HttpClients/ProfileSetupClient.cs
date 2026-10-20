using MeetCampus.Client.Infrastructure;
using System.Net.Http.Headers;
using MeetCampus.Contracts.Features.Profile;

namespace MeetCampus.Client.Features.Profile.HttpClients;

public class ProfileSetupClient(HttpClient httpClient) : BaseApiClient(httpClient), IProfileSetupClient
{
    private const string ProfileSetupEndpoint = "/api/profile/setup";
    private const string ProfileImagesEndpoint = "/api/profile/setup/images";

    public Task<UserProfileSetupResponse> GetProfileSetupAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<UserProfileSetupResponse>(ProfileSetupEndpoint, cancellationToken);
    }

    public Task UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return PutAsync(ProfileSetupEndpoint, request, cancellationToken);
    }

    public async Task UploadProfileImagesAsync(
        IReadOnlyList<ProfileImageUploadRequest> images,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(images);
        if (images.Count == 0)
            throw new ArgumentException("At least one profile image is required.", nameof(images));

        using var multipartContent = new MultipartFormDataContent();
        foreach (var image in images)
        {
            ArgumentNullException.ThrowIfNull(image);
            ArgumentNullException.ThrowIfNull(image.Content);
            ArgumentException.ThrowIfNullOrWhiteSpace(image.FileName);
            ArgumentException.ThrowIfNullOrWhiteSpace(image.ContentType);

            var streamContent = new StreamContent(image.Content);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
            multipartContent.Add(streamContent, "files", image.FileName);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, ProfileImagesEndpoint)
        {
            Content = multipartContent,
        };
        using var response = await SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task SetAvatarAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"{ProfileImagesEndpoint}/{imageId:D}/avatar");
        using var response = await SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteProfileImageAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"{ProfileImagesEndpoint}/{imageId:D}");
        using var response = await SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }
}
