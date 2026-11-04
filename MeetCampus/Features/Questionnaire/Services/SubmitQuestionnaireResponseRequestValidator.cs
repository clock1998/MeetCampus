using FluentValidation;
using MeetCampus.Contracts.Features.Questionnaire;

namespace MeetCampus.Features.Questionnaire.Services;

public sealed class SubmitQuestionnaireResponseRequestValidator : AbstractValidator<SubmitQuestionnaireResponseRequest>
{
    public SubmitQuestionnaireResponseRequestValidator()
    {
        RuleFor(request => request.Answers).NotNull();
        RuleFor(request => request.Answers)
            .Must(answers => answers is null
                || answers.Select(answer => answer.QuestionId).Distinct().Count() == answers.Count)
            .WithMessage("Each question may appear only once in a questionnaire submission.");
    }
}
