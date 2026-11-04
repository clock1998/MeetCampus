using FluentValidation;

namespace MeetCampus.Features.Questionnaire.Services;

public sealed class QuestionnaireCultureValidator : AbstractValidator<string>
{
    public QuestionnaireCultureValidator()
    {
        RuleFor(cultureCode => cultureCode).Must(cultureCode => cultureCode is "en" or "fr")
            .WithMessage("Supported questionnaire cultures are 'en' and 'fr'.");
    }
}
