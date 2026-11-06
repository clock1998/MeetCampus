using MeetCampus.Contracts.Features.Questionnaire;
using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Data;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.User;
using MeetCampus.Features.Questionnaire.Services;
using FluentValidation;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeetCampus.IntegrationTests.Features.Questionnaire;

public sealed class QuestionnaireResponseTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task WhenMemberSubmitsAnswersThenOnlyTheirCurrentAnswersAreReturned()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var member = await CreateUserAsync(dbContext);
        var otherMember = await CreateUserAsync(dbContext);
        var question = CreateQuestion();
        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = scope.ServiceProvider.GetRequiredService<QuestionnaireResponseService>();

        await service.SubmitAsync(member, new SubmitQuestionnaireResponseRequest
        ([
            new QuestionnaireAnswerRequest(question.Id, ["yes"], null)
        ]), TestContext.Current.CancellationToken);

        var memberResponse = await service.GetResponseAsync(member, "en", TestContext.Current.CancellationToken);
        var otherResponse = await service.GetResponseAsync(otherMember, "en", TestContext.Current.CancellationToken);

        Assert.Equal(["yes"], memberResponse.Questions.Single(item => item.Id == question.Id).SelectedOptionCodes);
        Assert.Empty(otherResponse.Questions.Single(item => item.Id == question.Id).SelectedOptionCodes);
    }

    [Fact]
    public async Task WhenQuestionDefinitionChangesThenExistingAnswerRequiresRefresh()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var member = await CreateUserAsync(dbContext);
        var question = CreateQuestion();
        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = scope.ServiceProvider.GetRequiredService<QuestionnaireResponseService>();
        await service.SubmitAsync(member, new SubmitQuestionnaireResponseRequest
        ([
            new QuestionnaireAnswerRequest(question.Id, ["yes"], null)
        ]), TestContext.Current.CancellationToken);

        question.UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(1);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var response = await service.GetResponseAsync(member, "en", TestContext.Current.CancellationToken);

        Assert.True(response.Questions.Single().IsRefreshRequired);
        Assert.False(response.IsComplete);
    }

    [Fact]
    public async Task WhenAnswerContainsOptionFromAnotherQuestionThenSubmissionIsRejected()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var member = await CreateUserAsync(dbContext);
        var question = CreateQuestion();
        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = scope.ServiceProvider.GetRequiredService<QuestionnaireResponseService>();

        await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitAsync(member, new SubmitQuestionnaireResponseRequest
        ([
            new QuestionnaireAnswerRequest(question.Id, ["missing"], null)
        ]), TestContext.Current.CancellationToken));

        Assert.Empty(await dbContext.UserAnswers.Where(answer => answer.ApplicationUserId == member.Id).ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WhenMemberSubmitsEmptyAnswerThenExistingAnswerIsClearedWithoutAffectingOtherQuestions()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var member = await CreateUserAsync(dbContext);
        var firstQuestion = CreateQuestion();
        var secondQuestion = CreateQuestion();
        firstQuestion.Code += "-1";
        secondQuestion.Code += "-2";
        dbContext.Questions.AddRange(firstQuestion, secondQuestion);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = scope.ServiceProvider.GetRequiredService<QuestionnaireResponseService>();
        await service.SubmitAsync(member, new SubmitQuestionnaireResponseRequest
        ([
            new QuestionnaireAnswerRequest(firstQuestion.Id, ["yes"], null),
            new QuestionnaireAnswerRequest(secondQuestion.Id, ["yes"], null)
        ]), TestContext.Current.CancellationToken);

        await service.SubmitAsync(member, new SubmitQuestionnaireResponseRequest
        ([
            new QuestionnaireAnswerRequest(firstQuestion.Id, [], null)
        ]), TestContext.Current.CancellationToken);

        var response = await service.GetResponseAsync(member, "en", TestContext.Current.CancellationToken);
        Assert.Empty(response.Questions.Single(question => question.Id == firstQuestion.Id).SelectedOptionCodes);
        Assert.Equal(["yes"], response.Questions.Single(question => question.Id == secondQuestion.Id).SelectedOptionCodes);
    }

    private ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(fixture.ConnectionString));
        services.AddScoped<IValidator<string>, QuestionnaireCultureValidator>();
        services.AddScoped<IValidator<SubmitQuestionnaireResponseRequest>, SubmitQuestionnaireResponseRequestValidator>();
        services.AddScoped<IValidator<QuestionnaireAnswerValidationContext>, QuestionnaireAnswerValidator>();
        services.AddScoped<QuestionnaireResponseService>();
        return services.BuildServiceProvider();
    }

    private static async Task<ApplicationUser> CreateUserAsync(ApplicationDbContext dbContext)
    {
        var email = $"question-response-{Guid.NewGuid():N}@example.edu";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return user;
    }

    private static Question CreateQuestion() => new()
    {
        Code = $"question-{Guid.NewGuid():N}",
        Type = QuestionType.SingleChoice,
        IsRequired = true,
        Category = "interests",
        ScoringWeight = 0.1m,
        Translations =
        [
            new QuestionTranslation { CultureCode = "en", Text = "Which activity?" },
            new QuestionTranslation { CultureCode = "fr", Text = "Quelle activité ?" }
        ],
        Options =
        [
            new QuestionOption
            {
                OptionCode = "yes",
                SortOrder = 0,
                Translations =
                [
                    new QuestionOptionTranslation { CultureCode = "en", Text = "Yes" },
                    new QuestionOptionTranslation { CultureCode = "fr", Text = "Oui" }
                ]
            },
            new QuestionOption
            {
                OptionCode = "no",
                SortOrder = 1,
                Translations =
                [
                    new QuestionOptionTranslation { CultureCode = "en", Text = "No" },
                    new QuestionOptionTranslation { CultureCode = "fr", Text = "Non" }
                ]
            }
        ]
    };
}
