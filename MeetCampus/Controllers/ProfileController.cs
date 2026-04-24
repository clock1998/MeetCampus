using MeetCampus.Data;
using MeetCampus.Services.Profile;
using MeetCampus.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeetCampus.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public sealed class ProfileController(
    ProfileSetupService profileSetupService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("setup")]
    public async Task<IActionResult> GetSetupAsync(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var response = await profileSetupService.GetSetupAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("setup")]
    public async Task<IActionResult> UpdateSetupAsync(
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
            await profileSetupService.UpdateSetupAsync(userId, request, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
