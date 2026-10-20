using MeetCampus.Contracts.Features.Profile.Contracts;

namespace MeetCampus.Client.Features.Profile.HttpClients;

public interface IProfileSetupClient
{
    Task<UserProfileSetupResponse> GetProfileSetupAsync(CancellationToken cancellationToken = default);

    Task UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default);

    Task UploadProfileImagesAsync(IReadOnlyList<ProfileImageUploadRequest> images, CancellationToken cancellationToken = default);

    Task SetAvatarAsync(Guid imageId, CancellationToken cancellationToken = default);

    Task DeleteProfileImageAsync(Guid imageId, CancellationToken cancellationToken = default);
}
