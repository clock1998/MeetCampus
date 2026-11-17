using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Contracts.Features.Auth;
using MeetCampus.Data;
using MeetCampus.Data.Profiles;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.User;
using MeetCampus.Features.Account.Services;
using MeetCampus.IntegrationTests.Infrastructure;
using MeetCampus.Features.Recommendations.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Xunit;
using MeetCampus.Configuration;
using Npgsql;

namespace MeetCampus.IntegrationTests.Features.Auth;

public sealed class AccountEligibilityServiceTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task WhenAccountHasNoEligibilityPrerequisitesThenStatusReportsIneligible()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"eligibility-{Guid.NewGuid():N}@example.edu";
        var user = new ApplicationUser { UserName = email, Email = email };
        var createResult = await userManager.CreateAsync(user, "Test-password1!");
        Assert.True(createResult.Succeeded);

        dbContext.AccountLifecycles.Add(new AccountLifecycle
        {
            ApplicationUserId = user.Id,
            Status = AccountStatus.Active,
            DiscoverabilityOptIn = true
        });
        dbContext.Questions.Add(new Question
        {
            Id = Guid.NewGuid(),
            Code = $"required-{Guid.NewGuid():N}",
            Type = QuestionType.SingleChoice,
            CategoryId = Guid.Parse("80000000-0000-0000-0000-000000000001"),
            ScoringWeight = 0.5m
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new AccountEligibilityService(
            dbContext,
            userManager,
            Options.Create(new ContactOptions()));
        var response = await service.GetEligibilityAsync(user, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(AccountEligibilityFailure.EmailNotVerified | AccountEligibilityFailure.ProfileIncomplete
            | AccountEligibilityFailure.ProfileImageMissing | AccountEligibilityFailure.AnswersIncomplete, response.Failures);
    }

    [Fact]
    public async Task WhenPreviouslyOptionalQuestionIsUnansweredThenMemberIsIneligible()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await CreateEligibleMemberAsync(dbContext, userManager, validAnswer: true);
        dbContext.Questions.Add(new Question
        {
            Id = Guid.NewGuid(), Code = $"required-{Guid.NewGuid():N}", Type = QuestionType.Scale,
            CategoryId = Guid.Parse("80000000-0000-0000-0000-000000000001"),
            ScoringWeight = 0.5m,
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = new AccountEligibilityService(dbContext, userManager, Options.Create(new ContactOptions()));

        var result = await service.GetEligibilityAsync(user, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.True(result.Failures.HasFlag(AccountEligibilityFailure.AnswersIncomplete));
    }

    [Fact]
    public async Task WhenMemberIsEligibleThenRecommendationAvailabilityIsTrue()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await CreateEligibleMemberAsync(dbContext, userManager, validAnswer: true);
        var controller = new RecommendationsController(
            new AccountEligibilityService(dbContext, userManager, Options.Create(new ContactOptions())),
            userManager);
        controller.ControllerContext = CreateControllerContext(user.Id);

        var result = await controller.GetAvailabilityAsync(TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MeetCampus.Contracts.Features.Recommendations.RecommendationAvailabilityResponse>(okResult.Value);
        Assert.True(response.IsAvailable);
    }

    [Fact]
    public async Task WhenMemberIsIneligibleThenRecommendationAvailabilityIsFalse()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"ineligible-{Guid.NewGuid():N}@example.edu";
        var user = new ApplicationUser { UserName = email, Email = email };
        var createResult = await userManager.CreateAsync(user, "Test-password1!");
        Assert.True(createResult.Succeeded);
        var controller = new RecommendationsController(
            new AccountEligibilityService(dbContext, userManager, Options.Create(new ContactOptions())),
            userManager);
        controller.ControllerContext = CreateControllerContext(user.Id);

        var result = await controller.GetAvailabilityAsync(TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MeetCampus.Contracts.Features.Recommendations.RecommendationAvailabilityResponse>(okResult.Value);
        Assert.False(response.IsAvailable);
    }

    private static ControllerContext CreateControllerContext(Guid userId) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                "Test"))
        }
    };

    [Fact]
    public async Task WhenMemberHasEveryCurrentPrerequisiteThenEligibilityReflectsPersistedChanges()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await CreateEligibleMemberAsync(dbContext, userManager, validAnswer: true);
        var service = new AccountEligibilityService(dbContext, userManager, Options.Create(new ContactOptions()));

        var eligibleResponse = await service.GetEligibilityAsync(user, TestContext.Current.CancellationToken);

        Assert.NotNull(eligibleResponse);
        Assert.True(eligibleResponse.IsEligibleForDiscoverability);

        var lifecycle = await dbContext.AccountLifecycles.SingleAsync(item => item.ApplicationUserId == user.Id, TestContext.Current.CancellationToken);
        lifecycle.DiscoverabilityOptIn = false;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var optedOutResponse = await service.GetEligibilityAsync(user, TestContext.Current.CancellationToken);

        Assert.NotNull(optedOutResponse);
        Assert.True(optedOutResponse.Failures.HasFlag(AccountEligibilityFailure.DiscoverabilityNotOptedIn));
        Assert.False(optedOutResponse.IsEligibleForDiscoverability);
    }

    [Fact]
    public async Task WhenRequiredAnswerHasAnInvalidStoredShapeThenMemberIsIneligible()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await CreateEligibleMemberAsync(dbContext, userManager, validAnswer: false);
        var service = new AccountEligibilityService(dbContext, userManager, Options.Create(new ContactOptions()));

        var response = await service.GetEligibilityAsync(user, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.True(response.Failures.HasFlag(AccountEligibilityFailure.AnswersIncomplete));
        Assert.False(response.IsEligibleForDiscoverability);
    }

    private async Task<ServiceProvider> CreateServicesAsync()
    {
        var databaseName = $"account_eligibility_{Guid.NewGuid():N}";
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection);
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        var connectionString = new NpgsqlConnectionStringBuilder(fixture.ConnectionString) { Database = databaseName }.ConnectionString;
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        return services.BuildServiceProvider();
    }

    private static async Task<ApplicationUser> CreateEligibleMemberAsync(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        bool validAnswer)
    {
        await dbContext.UserAnswers.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
        await dbContext.Questions.ExecuteDeleteAsync(TestContext.Current.CancellationToken);

        var email = $"eligible-{Guid.NewGuid():N}@example.edu";
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var createResult = await userManager.CreateAsync(user, "Test-password1!");
        Assert.True(createResult.Succeeded);
        dbContext.AccountLifecycles.Add(new AccountLifecycle
        {
            ApplicationUserId = user.Id,
            Status = AccountStatus.Active,
            DiscoverabilityOptIn = true
        });
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = user.Id,
            StudyDomainId = Guid.Parse("10000000-0000-0000-0000-000000000011"),
            SchoolId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            LanguageId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
            EthnicityId = Guid.Parse("40000000-0000-0000-0000-000000000001")
        };
        dbContext.UserProfiles.Add(profile);
        dbContext.UserProfileIntentions.Add(new UserProfileIntention
        {
            UserProfileId = profile.Id,
            UserIntentionId = Guid.Parse("30000000-0000-0000-0000-000000000001")
        });
        dbContext.UserProfileImages.Add(new UserProfileImage
        {
            Id = Guid.NewGuid(),
            UserProfileId = profile.Id,
            ImageKey = $"eligibility/{user.Id:N}.webp",
            IsAvatar = true
        });
        var option = new QuestionOption
        {
            Id = Guid.NewGuid(),
            OptionCode = "yes",
            SortOrder = 0
        };
        var question = new Question
        {
            Id = Guid.NewGuid(),
            Code = $"required-{Guid.NewGuid():N}",
            Type = QuestionType.SingleChoice,
            CategoryId = Guid.Parse("80000000-0000-0000-0000-000000000001"),
            ScoringWeight = 0.5m,
            Options = [option]
        };
        dbContext.Questions.Add(question);
        dbContext.UserAnswers.Add(new UserAnswer
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = user.Id,
            QuestionId = question.Id,
            QuestionOptionId = validAnswer ? option.Id : null,
            ScaleValue = validAnswer ? null : 1,
            UpdatedAt = question.UpdatedAt
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return user;
    }
}
