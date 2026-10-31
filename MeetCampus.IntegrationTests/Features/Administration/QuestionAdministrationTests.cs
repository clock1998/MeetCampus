using System.Security.Claims;
using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Data;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.User;
using MeetCampus.Features.Administration.Services;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeetCampus.IntegrationTests.Features.Administration;

public sealed class QuestionAdministrationTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task WhenQuestionIsUpdatedThenTimestampChangesAndAuditIsWritten()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var administrator = await CreateUserAsync(dbContext);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, administrator.Id.ToString()),
            new Claim(ClaimTypes.Role, "Administrator")
        ], "integration-test"));
        var service = scope.ServiceProvider.GetRequiredService<AdministrativeDataService>();
        var question = new Question
        {
            Code = $"q-{Guid.NewGuid():N}",
            Type = QuestionType.SingleChoice,
            Translations =
            [
                new QuestionTranslation { CultureCode = "en", Text = "Test question" },
                new QuestionTranslation { CultureCode = "fr", Text = "Question de test" }
            ],
            Group = "interests",
            ScoringWeight = 0.1m,
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1),
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
                }
            ]
        };
        await service.CreateQuestionAsync(principal, question, TestContext.Current.CancellationToken);
        var oldUpdatedAt = question.UpdatedAt;

        await service.UpdateQuestionAsync(principal, question.Id, new Question
        {
            Code = question.Code,
            Type = QuestionType.SingleChoice,
            Translations =
            [
                new QuestionTranslation { CultureCode = "en", Text = "Updated test question" },
                new QuestionTranslation { CultureCode = "fr", Text = "Question de test modifiée" }
            ],
            Group = question.Group,
            ScoringWeight = question.ScoringWeight,
            Options = question.Options.ToList()
        }, TestContext.Current.CancellationToken);

        var updatedQuestion = await dbContext.Questions.SingleAsync(item => item.Id == question.Id, TestContext.Current.CancellationToken);
        Assert.True(updatedQuestion.UpdatedAt > oldUpdatedAt);
        Assert.True(await dbContext.AuditEvents.AnyAsync(item => item.Action == "Question.Updated" && item.EntityId == question.Id.ToString(), TestContext.Current.CancellationToken));
    }

    private ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options => options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator")));
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(fixture.ConnectionString));
        services.AddScoped<AdministrativeDataService>();
        return services.BuildServiceProvider();
    }

    private static async Task<ApplicationUser> CreateUserAsync(ApplicationDbContext dbContext)
    {
        var email = $"question-admin-{Guid.NewGuid():N}@example.edu";
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
}
