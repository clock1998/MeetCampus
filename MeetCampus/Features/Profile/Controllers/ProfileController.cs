using MeetCampus.Data.User;
using MeetCampus.Features.Profile.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MeetCampus.Contracts.Features.Profile;

namespace MeetCampus.Features.Profile.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public sealed class ProfileController(
    UserProfileService userProfileService,
    ProfileImageService profileImageService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("setup")]
    public async Task<IActionResult> GetProfileSetupAsync(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var response = await userProfileService.GetProfileSetupAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("setup")]
    public async Task<IActionResult> UpdateProfileAsync(
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            await userProfileService.UpdateProfileAsync(userId, request, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("setup/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ProfileImageService.MaximumUploadRequestSizeBytes)]
    public async Task<IActionResult> UploadProfileImagesAsync(
        [FromForm] List<IFormFile>? files,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (files is null || files.Count == 0)
        {
            return BadRequest("At least one image file is required.");
        }

        try
        {
            await profileImageService.UploadProfileImagesAsync(userId, files, cancellationToken);
            return NoContent();
        }
        catch (InvalidDataException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("setup/images/{imageId:guid}/avatar")]
    public async Task<IActionResult> SetAvatarAsync(Guid imageId, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var wasUpdated = await profileImageService.SetAvatarAsync(userId, imageId, cancellationToken);
        return wasUpdated ? NoContent() : NotFound();
    }

    [HttpDelete("setup/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteProfileImageAsync(Guid imageId, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var wasDeleted = await profileImageService.DeleteProfileImageAsync(userId, imageId, cancellationToken);
        return wasDeleted ? NoContent() : NotFound();
    }
}
