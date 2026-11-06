using MeetCampus.Contracts.Features.Recommendations;
using MeetCampus.Data.User;
using MeetCampus.Features.Account.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeetCampus.Features.Recommendations.Controllers;

[ApiController]
[Route("api/recommendations")]
[Authorize]
public sealed class RecommendationsController(
    AccountEligibilityService accountEligibilityService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("availability")]
    public async Task<ActionResult<RecommendationAvailabilityResponse>> GetAvailabilityAsync(
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Unauthorized();

        var eligibility = await accountEligibilityService.GetEligibilityAsync(user.Id, cancellationToken);
        return eligibility is null
            ? Unauthorized()
            : Ok(new RecommendationAvailabilityResponse(eligibility.IsEligibleForDiscoverability));
    }
}
