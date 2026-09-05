using MeetCampus.Contracts.Features.Profile.Contracts;
using MeetCampus.HttpClients.Infrastructure.Http;

namespace MeetCampus.HttpClients.Features.Profile;

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
