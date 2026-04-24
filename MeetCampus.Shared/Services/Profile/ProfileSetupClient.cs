using MeetCampus.Shared.Models;
using MeetCampus.Shared.Services.Http;

namespace MeetCampus.Shared.Services.Profile;

public class ProfileSetupClient(HttpClient httpClient) : BaseApiClient(httpClient), IProfileSetupClient
{
    private const string ProfileSetupEndpoint = "/api/profile/setup";

    public Task<UserProfileSetupResponse> GetSetupAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<UserProfileSetupResponse>(ProfileSetupEndpoint, cancellationToken);
    }

    public Task UpdateSetupAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return PutAsync(ProfileSetupEndpoint, request, cancellationToken);
    }
}
