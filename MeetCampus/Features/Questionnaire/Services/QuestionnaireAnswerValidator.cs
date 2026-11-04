using FluentValidation;
using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Contracts.Features.Questionnaire;
using MeetCampus.Data.Questionnaire;

namespace MeetCampus.Features.Questionnaire.Services;

public sealed class QuestionnaireAnswerValidator : AbstractValidator<QuestionnaireAnswerValidationContext>
{
    public QuestionnaireAnswerValidator()
    {
        RuleFor(context => context.Question).NotNull();
        RuleFor(context => context.Answer).NotNull();
        RuleFor(context => context.Answer.OptionCodes)
            .NotNull()
            .Must(optionCodes => optionCodes is null
                || optionCodes.Distinct(StringComparer.Ordinal).Count() == optionCodes.Count)
            .WithMessage("An option can only be selected once per question.");
        RuleFor(context => context)
            .Must(context => context.Answer.OptionCodes is null
                || (QuestionType)context.Question.Type switch
                {
                    QuestionType.SingleChoice => !context.Answer.ScaleValue.HasValue && context.Answer.OptionCodes.Count <= 1,
                    QuestionType.MultipleChoice => !context.Answer.ScaleValue.HasValue
                        && (!context.Question.MaxSelections.HasValue
                            || context.Answer.OptionCodes.Count <= context.Question.MaxSelections.Value),
                    QuestionType.Scale => context.Answer.OptionCodes.Count == 0
                        && (!context.Answer.ScaleValue.HasValue
                            || context.Answer.ScaleValue >= context.Question.ScaleMin
                                && context.Answer.ScaleValue <= context.Question.ScaleMax),
                    _ => false
                })
            .WithMessage("The answer does not match the question type or configured constraints.");
        RuleFor(context => context)
            .Must(context => context.Answer.OptionCodes is null
                || (QuestionType)context.Question.Type == QuestionType.Scale
                || context.Answer.OptionCodes.All(code => context.Question.Options.Any(option => option.OptionCode == code)))
            .WithMessage("The answer contains an option that does not belong to the question.");
    }
}

public sealed record QuestionnaireAnswerValidationContext(Question Question, QuestionnaireAnswerRequest Answer);
