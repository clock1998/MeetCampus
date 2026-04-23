using MeetCampus.Shared.Models;

namespace MeetCampus.Client.Services.Profile;

internal interface IProfileSetupClient
{
    Task<UserProfileSetupResponse> GetSetupAsync(CancellationToken cancellationToken = default);

    Task UpdateSetupAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
}
