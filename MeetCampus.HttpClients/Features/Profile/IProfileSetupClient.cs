using MeetCampus.Contracts.Features.Profile.Contracts;

namespace MeetCampus.HttpClients.Features.Profile;

public interface IProfileSetupClient
{
    Task<UserProfileSetupResponse> GetSetupAsync(CancellationToken cancellationToken = default);

    Task UpdateSetupAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
}
