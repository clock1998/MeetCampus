using FluentValidation.TestHelper;
using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Contracts.Features.Questionnaire;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Features.Administration.Services;
using MeetCampus.Features.Questionnaire.Services;
using MeetCampus.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeetCampus.Tests.Features.Questionnaire;

public sealed class QuestionnaireValidatorsTests
{
    [Fact]
    public async Task WhenQuestionDefinitionHasIncompleteTranslationsThenValidationFails()
    {
        await using var dbContext = CreateDbContext();
        var validator = new QuestionnaireDefinitionValidator(dbContext);
        var question = CreateQuestion();
        question.CategoryId = Guid.Empty;
        question.Translations.Remove(question.Translations.Last());

        var result = await validator.TestValidateAsync(question);

        result.ShouldHaveValidationErrorFor(item => item.Translations);
    }

    [Fact]
    public async Task WhenQuestionDefinitionHasNoCategoryThenValidationFails()
    {
        await using var dbContext = CreateDbContext();
        var validator = new QuestionnaireDefinitionValidator(dbContext);
        var question = CreateQuestion();
        question.CategoryId = Guid.Empty;

        var result = await validator.TestValidateAsync(question);

        result.ShouldHaveValidationErrorFor(item => item.CategoryId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task WhenQuestionWeightIsAtBoundaryThenValidationFails(double weight)
    {
        await using var dbContext = CreateDbContext();
        var validator = new QuestionnaireDefinitionValidator(dbContext);
        var question = CreateQuestion();
        question.CategoryId = Guid.Empty;
        question.ScoringWeight = (decimal)weight;

        var result = await validator.TestValidateAsync(question);

        result.ShouldHaveValidationErrorFor(item => item.ScoringWeight);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(0.5)]
    public void WhenRequestWeightIsValidatedThenExclusiveBoundsApply(double weight)
    {
        var request = new QuestionRequest { ScoringWeight = (decimal)weight };
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(request) { MemberName = nameof(request.ScoringWeight) };

        var valid = System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(request.ScoringWeight, context, []);

        Assert.Equal(weight > 0 && weight < 1, valid);
    }

    private static ApplicationDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=validation-test;Username=test;Password=test;Timeout=1")
            .Options);

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
        CategoryId = Guid.NewGuid(),
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
