using FluentValidation;
using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Data.Questionnaire;

namespace MeetCampus.Features.Administration.Services;

public sealed class QuestionnaireDefinitionValidator : AbstractValidator<Question>
{
    public QuestionnaireDefinitionValidator()
    {
        RuleFor(question => question.Code).NotEmpty();
        RuleFor(question => question.Group)
            .NotEmpty()
            .Must(group => QuestionnaireAdminCatalog.Value.Categories.Any(category => category.Code == group))
            .WithMessage("Question category is not supported.");
        RuleFor(question => question.Type).IsInEnum();
        RuleFor(question => question.ScoringWeight).InclusiveBetween(0, 1);
        RuleFor(question => question.Translations)
            .Must(HasEnglishAndFrenchTranslations)
            .WithMessage("Question prompts are required in English and French.");
        RuleFor(question => question)
            .Must(question => question.Type == QuestionType.Scale || question.Options.Count > 0)
            .WithMessage("Choice questions must define at least one option.");
        RuleFor(question => question)
            .Must(question => question.Type != QuestionType.Scale
                || question.Options.Count == 0
                    && question.ScaleMin.HasValue
                    && question.ScaleMax.HasValue
                    && question.ScaleMin < question.ScaleMax
                    && !question.MaxSelections.HasValue)
            .WithMessage("Scale questions require a valid range and cannot define choices or a selection limit.");
        RuleFor(question => question)
            .Must(question => question.Type == QuestionType.Scale
                || !question.ScaleMin.HasValue && !question.ScaleMax.HasValue)
            .WithMessage("Only scale questions may define scale bounds.");
        RuleFor(question => question)
            .Must(question => question.Type == QuestionType.MultipleChoice || !question.MaxSelections.HasValue)
            .WithMessage("Only multiple-choice questions may define a selection limit.");
        RuleFor(question => question)
            .Must(question => !question.MaxSelections.HasValue
                || question.MaxSelections > 0 && question.MaxSelections <= question.Options.Count)
            .WithMessage("The selection limit must be positive and no greater than the option count.");
        RuleForEach(question => question.Options).ChildRules(option =>
        {
            option.RuleFor(item => item.OptionCode).NotEmpty();
            option.RuleFor(item => item.SortOrder).GreaterThanOrEqualTo(0);
            option.RuleFor(item => item.Translations)
                .Must(HasEnglishAndFrenchTranslations)
                .WithMessage("Question options require stable codes and English and French labels.");
        });
        RuleFor(question => question.Options)
            .Must(options => options.Select(option => option.OptionCode).Distinct(StringComparer.Ordinal).Count() == options.Count)
            .WithMessage("Question option codes must be unique.");
        RuleFor(question => question.Options)
            .Must(options => options.Select(option => option.SortOrder).Distinct().Count() == options.Count)
            .WithMessage("Question option sort orders must be unique.");
    }

    private static bool HasEnglishAndFrenchTranslations<TTranslation>(ICollection<TTranslation> translations)
        where TTranslation : class
    {
        if (translations is null)
            return false;

        var items = translations.Select(translation => translation switch
        {
            QuestionTranslation questionTranslation => (CultureCode: questionTranslation.CultureCode, Text: questionTranslation.Text),
            QuestionOptionTranslation optionTranslation => (CultureCode: optionTranslation.CultureCode, Text: optionTranslation.Text),
            _ => (CultureCode: string.Empty, Text: string.Empty)
        }).ToList();

        return items.Count == 2
            && items.All(item => !string.IsNullOrWhiteSpace(item.Text))
            && items.Select(item => item.CultureCode).ToHashSet(StringComparer.Ordinal).SetEquals(["en", "fr"]);
    }
}
