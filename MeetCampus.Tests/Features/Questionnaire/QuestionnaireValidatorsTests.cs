using FluentValidation.TestHelper;
using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Contracts.Features.Questionnaire;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Features.Administration.Services;
using MeetCampus.Features.Questionnaire.Services;
using Xunit;

namespace MeetCampus.Tests.Features.Questionnaire;

public sealed class QuestionnaireValidatorsTests
{
    [Fact]
    public void WhenQuestionDefinitionHasIncompleteTranslationsThenValidationFails()
    {
        var validator = new QuestionnaireDefinitionValidator();
        var question = CreateQuestion();
        question.Translations.Remove(question.Translations.Last());

        var result = validator.TestValidate(question);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void WhenScaleAnswerIsWithinRangeThenValidationSucceeds()
    {
        var validator = new QuestionnaireAnswerValidator();
        var question = CreateQuestion();
        question.Type = QuestionType.Scale;
        question.Options.Clear();
        question.ScaleMin = 1;
        question.ScaleMax = 5;
        var answer = new QuestionnaireAnswerRequest(question.Id, [], 3);

        var result = validator.TestValidate(new QuestionnaireAnswerValidationContext(question, answer));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void WhenAnswerUsesUnknownOptionThenValidationFails()
    {
        var validator = new QuestionnaireAnswerValidator();
        var question = CreateQuestion();
        var answer = new QuestionnaireAnswerRequest(question.Id, ["missing"], null);

        var result = validator.TestValidate(new QuestionnaireAnswerValidationContext(question, answer));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void WhenQuestionnaireCultureIsUnsupportedThenValidationFails()
    {
        var validator = new QuestionnaireCultureValidator();

        var result = validator.TestValidate("es");

        Assert.False(result.IsValid);
    }

    private static Question CreateQuestion() => new()
    {
        Id = Guid.NewGuid(),
        Code = "test-question",
        Type = QuestionType.SingleChoice,
        IsRequired = true,
        Group = "interests",
        ScoringWeight = 0.5m,
        Translations =
        [
            new QuestionTranslation { CultureCode = "en", Text = "Question" },
            new QuestionTranslation { CultureCode = "fr", Text = "Question FR" }
        ],
        Options =
        [
            new QuestionOption
            {
                Id = Guid.NewGuid(),
                QuestionId = Guid.NewGuid(),
                OptionCode = "yes",
                SortOrder = 0,
                Translations =
                [
                    new QuestionOptionTranslation { CultureCode = "en", Text = "Yes" },
                    new QuestionOptionTranslation { CultureCode = "fr", Text = "Oui" }
                ]
            }
        ]
    };
}
