using MeetCampus.Contracts.Features.Questionnaire;
using MeetCampus.Data.User;
using MeetCampus.Features.Questionnaire.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeetCampus.Features.Questionnaire.Controllers;

[ApiController]
[Route("api/questionnaire")]
[Authorize]
public sealed class QuestionnaireController(
    QuestionnaireResponseService questionnaireResponseService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<QuestionnaireResponse>> GetAsync(
        [FromQuery] string culture = "en",
        CancellationToken cancellationToken = default)
    {
        var actor = await userManager.GetUserAsync(User);
        if (actor is null)
            return Unauthorized();

        try
        {
            return Ok(await questionnaireResponseService.GetResponseAsync(actor, culture, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Unsupported questionnaire culture", Detail = exception.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> SubmitAsync(
        SubmitQuestionnaireResponseRequest request,
        CancellationToken cancellationToken)
    {
        var member = await userManager.GetUserAsync(User);
        if (member is null)
            return Unauthorized();

        try
        {
            await questionnaireResponseService.SubmitAsync(member, request, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid questionnaire response", Detail = exception.Message });
        }
    }
}
