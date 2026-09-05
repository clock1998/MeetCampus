using MeetCampus.Contracts.Models;

namespace MeetCampus.HttpClients.Profile;

public interface IProfileSetupClient
{
    Task<UserProfileSetupResponse> GetSetupAsync(CancellationToken cancellationToken = default);

    Task UpdateSetupAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
}
