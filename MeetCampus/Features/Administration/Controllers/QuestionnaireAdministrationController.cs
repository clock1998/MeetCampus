using System.Security.Claims;
using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Features.Administration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetCampus.Features.Administration.Controllers;

[ApiController]
[Route("api/admin/questionnaire")]
[Authorize(Policy = "AdministratorOnly")]
public sealed class QuestionnaireAdministrationController(AdministrativeDataService administrativeDataService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<QuestionResponse>>> GetQuestionsAsync(CancellationToken cancellationToken)
    {
        var questions = await administrativeDataService.GetQuestionsAsync(User, cancellationToken);
        return Ok(questions.Select(ToResponse).ToList());
    }

    [HttpGet("catalog")]
    public async Task<ActionResult<QuestionCatalogResponse>> GetCatalogAsync(CancellationToken cancellationToken)
    {
        return Ok(await administrativeDataService.GetQuestionnaireAdminCatalogAsync(User));
    }

    [HttpPost]
    public async Task<ActionResult<QuestionResponse>> CreateQuestionAsync(
        QuestionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var question = await administrativeDataService.CreateQuestionAsync(User, ToEntity(request), cancellationToken);
            return CreatedAtAction(nameof(GetQuestionsAsync), new { }, ToResponse(question));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid questionnaire definition", Detail = exception.Message });
        }
    }

    [HttpPut("{questionId:guid}")]
    public async Task<IActionResult> UpdateQuestionAsync(
        Guid questionId,
        QuestionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await administrativeDataService.UpdateQuestionAsync(User, questionId, ToEntity(request), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ProblemDetails { Title = "Question not found", Detail = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid questionnaire definition", Detail = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Question cannot be updated", Detail = exception.Message });
        }
    }

    [HttpDelete("{questionId:guid}")]
    public async Task<IActionResult> DeleteQuestionAsync(Guid questionId, CancellationToken cancellationToken)
    {
        try
        {
            await administrativeDataService.DeleteQuestionAsync(User, questionId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ProblemDetails { Title = "Question not found", Detail = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Question cannot be deleted", Detail = exception.Message });
        }
    }

    private static Question ToEntity(QuestionRequest request)
    {
        return new Question
        {
            Code = request.Code.Trim(),
            Type = (QuestionType)request.Type,
            IsRequired = request.IsRequired,
            Group = request.CategoryCode,
            ScoringWeight = request.ScoringWeight,
            SortOrder = request.SortOrder,
            MaxSelections = request.MaxSelections,
            ScaleMin = request.ScaleMin,
            ScaleMax = request.ScaleMax,
            Translations =
            [
                new QuestionTranslation { CultureCode = "en", Text = request.PromptEn.Trim() },
                new QuestionTranslation { CultureCode = "fr", Text = request.PromptFr.Trim() }
            ],
            Options = request.Options.Select(option => new QuestionOption
            {
                OptionCode = option.Code.Trim(),
                SortOrder = option.SortOrder,
                ScoringValue = option.ScoringValue,
                Translations =
                [
                    new QuestionOptionTranslation { CultureCode = "en", Text = option.LabelEn.Trim() },
                    new QuestionOptionTranslation { CultureCode = "fr", Text = option.LabelFr.Trim() }
                ]
            }).ToList()
        };
    }

    private static QuestionResponse ToResponse(Question question)
    {
        var translations = question.Translations.ToDictionary(item => item.CultureCode, item => item.Text, StringComparer.Ordinal);
        return new QuestionResponse(
            question.Id,
            question.Code,
            (QuestionType)question.Type,
            translations.GetValueOrDefault("en", string.Empty),
            translations.GetValueOrDefault("fr", string.Empty),
            question.IsRequired,
            question.Group,
            question.ScoringWeight,
            question.SortOrder,
            question.MaxSelections,
            question.ScaleMin,
            question.ScaleMax,
            question.Options.OrderBy(option => option.SortOrder).Select(option =>
            {
                var labels = option.Translations.ToDictionary(item => item.CultureCode, item => item.Text, StringComparer.Ordinal);
                return new QuestionOptionAdminResponse(
                    option.OptionCode,
                    labels.GetValueOrDefault("en", string.Empty),
                    labels.GetValueOrDefault("fr", string.Empty),
                    option.SortOrder,
                    option.ScoringValue);
            }).ToList(),
            question.UpdatedAt);
    }
}
