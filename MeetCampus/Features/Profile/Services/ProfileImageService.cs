using Amazon.S3;
using MeetCampus.Contracts.Features.Profile.Contracts;
using MeetCampus.Data;
using MeetCampus.Data.Profiles;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace MeetCampus.Features.Profile.Services;

public sealed class ProfileImageService(
    ApplicationDbContext dbContext,
    S3ProfileImageStorage imageStorage,
    ILogger<ProfileImageService> logger)
{
    internal const int MaximumProfileImageCount = 6;
    internal const long MaximumProfileImageSizeBytes = 5 * 1024 * 1024;
    internal const long MaximumUploadRequestSizeBytes = MaximumProfileImageCount * MaximumProfileImageSizeBytes + 128 * 1024;
    private const int MaximumAvatarEdgePixels = 512;
    private const long MaximumSourcePixelCount = 20_000_000;

    public async Task<IReadOnlyList<ProfileImageResponse>> GetProfileImagesAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var images = await dbContext.UserProfileImages
            .AsNoTracking()
            .Where(image => image.UserProfile.ApplicationUserId == userId)
            .OrderByDescending(image => image.IsAvatar)
            .ThenBy(image => image.Id)
            .Select(image => new { image.Id, image.ImageKey, image.IsAvatar })
            .ToListAsync(cancellationToken);

        return images
            .Select(image => new ProfileImageResponse(image.Id, imageStorage.GetReadUrl(image.ImageKey), image.IsAvatar))
            .ToList();
    }

    public async Task UploadProfileImagesAsync(
        string userId,
        IReadOnlyList<IFormFile> files,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(files);

        if (files.Count is < 1 or > MaximumProfileImageCount)
            throw new InvalidDataException($"Select between 1 and {MaximumProfileImageCount} images.");

        var profile = await dbContext.UserProfiles
            .Include(userProfile => userProfile.ProfileImages)
            .SingleOrDefaultAsync(userProfile => userProfile.ApplicationUserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Save profile details before uploading profile images.");

        profile.ProfileImages ??= [];
        if (profile.ProfileImages.Count + files.Count > MaximumProfileImageCount)
            throw new InvalidOperationException($"A profile can contain up to {MaximumProfileImageCount} images.");

        var hasAvatar = profile.ProfileImages.Any(image => image.IsAvatar);
        var uploadedKeys = new List<string>(files.Count);
        var persisted = false;

        try
        {
            foreach (var file in files)
            {
                using var optimizedImage = await OptimizeImageAsync(file, cancellationToken);
                var key = $"profile-images/{profile.Id}/{Guid.NewGuid():N}.webp";
                await imageStorage.UploadAsync(key, optimizedImage, "image/webp", cancellationToken);
                uploadedKeys.Add(key);

                var profileImage = new UserProfileImage
                {
                    Id = Guid.NewGuid(),
                    UserProfileId = profile.Id,
                    ImageKey = key,
                    IsAvatar = !hasAvatar,
                };
                dbContext.UserProfileImages.Add(profileImage);
                hasAvatar = true;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            persisted = true;
        }
        finally
        {
            if (!persisted)
            {
                foreach (var key in uploadedKeys)
                    await DeleteUploadedImageAsync(key);
            }
        }
    }

    public async Task<bool> SetAvatarAsync(string userId, Guid imageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var profile = await dbContext.UserProfiles
            .Include(userProfile => userProfile.ProfileImages)
            .SingleOrDefaultAsync(userProfile => userProfile.ApplicationUserId == userId, cancellationToken);
        var profileImages = profile?.ProfileImages;
        if (profileImages is null)
            return false;

        var selectedImage = profileImages.SingleOrDefault(image => image.Id == imageId);
        if (selectedImage is null)
            return false;

        foreach (var image in profileImages)
            image.IsAvatar = image.Id == selectedImage.Id;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteProfileImageAsync(string userId, Guid imageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var profile = await dbContext.UserProfiles
            .Include(userProfile => userProfile.ProfileImages)
            .SingleOrDefaultAsync(userProfile => userProfile.ApplicationUserId == userId, cancellationToken);
        var profileImages = profile?.ProfileImages;
        var image = profileImages?.SingleOrDefault(profileImage => profileImage.Id == imageId);
        if (profileImages is null || image is null)
            return false;

        var wasAvatar = image.IsAvatar;
        var imageKey = image.ImageKey;
        profileImages.Remove(image);
        dbContext.UserProfileImages.Remove(image);

        if (wasAvatar)
        {
            var nextAvatar = profileImages.OrderBy(profileImage => profileImage.Id).FirstOrDefault();
            if (nextAvatar is not null)
                nextAvatar.IsAvatar = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await imageStorage.DeleteAsync(imageKey, CancellationToken.None);
        }
        catch (AmazonS3Exception exception)
        {
            logger.LogError(exception, "Could not delete profile image object {ImageKey} after deleting its database record.", imageKey);
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "Could not delete profile image object {ImageKey} after deleting its database record.", imageKey);
        }

        return true;
    }

    private static async Task<MemoryStream> OptimizeImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.Length is <= 0 or > MaximumProfileImageSizeBytes)
            throw new InvalidDataException("Each image must be between 1 byte and 5 MiB.");

        using var imageContent = new MemoryStream((int)file.Length);
        await using (var fileStream = file.OpenReadStream())
            await fileStream.CopyToAsync(imageContent, cancellationToken);

        if (imageContent.Length != file.Length)
            throw new InvalidDataException("The uploaded image data is incomplete.");

        imageContent.Position = 0;
        var detectedFormat = await Image.DetectFormatAsync(imageContent, cancellationToken);
        if (detectedFormat != JpegFormat.Instance
            && detectedFormat != PngFormat.Instance
            && detectedFormat != WebpFormat.Instance)
        {
            throw new InvalidDataException("Only JPEG, PNG, and WebP images are supported.");
        }

        imageContent.Position = 0;
        var imageInfo = await Image.IdentifyAsync(imageContent, cancellationToken);
        if (imageInfo is null
            || imageInfo.Width <= 0
            || imageInfo.Height <= 0
            || (long)imageInfo.Width * imageInfo.Height > MaximumSourcePixelCount)
        {
            throw new InvalidDataException("The image dimensions are not supported.");
        }

        imageContent.Position = 0;
        using var image = await Image.LoadAsync(imageContent, cancellationToken);
        if (image.Width > MaximumAvatarEdgePixels || image.Height > MaximumAvatarEdgePixels)
        {
            image.Mutate(context => context.Resize(new ResizeOptions
            {
                Size = new Size(MaximumAvatarEdgePixels, MaximumAvatarEdgePixels),
                Mode = ResizeMode.Max,
            }));
        }

        var optimizedImage = new MemoryStream();
        var completed = false;
        try
        {
            await image.SaveAsWebpAsync(optimizedImage, new WebpEncoder { Quality = 80 }, cancellationToken);
            optimizedImage.Position = 0;
            completed = true;
            return optimizedImage;
        }
        finally
        {
            if (!completed)
                await optimizedImage.DisposeAsync();
        }
    }

    private async Task DeleteUploadedImageAsync(string key)
    {
        try
        {
            await imageStorage.DeleteAsync(key, CancellationToken.None);
        }
        catch (AmazonS3Exception exception)
        {
            logger.LogError(exception, "Could not clean up uploaded profile image {ImageKey} after a database failure.", key);
        }
    }
}
