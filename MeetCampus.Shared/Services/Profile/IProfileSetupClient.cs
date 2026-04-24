using MeetCampus.Shared.Models;

namespace MeetCampus.Shared.Services.Profile;

public interface IProfileSetupClient
{
    Task<UserProfileSetupResponse> GetSetupAsync(CancellationToken cancellationToken = default);

    Task UpdateSetupAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
}
