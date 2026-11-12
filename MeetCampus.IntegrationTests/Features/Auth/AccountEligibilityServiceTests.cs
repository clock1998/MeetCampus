using MeetCampus.Contracts.Features.Administration;
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

namespace MeetCampus.IntegrationTests.Features.Auth;

public sealed class AccountEligibilityServiceTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task WhenAccountHasNoEligibilityPrerequisitesThenStatusReportsIneligible()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);

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
            Category = "eligibility",
            ScoringWeight = 1
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new AccountEligibilityService(
            dbContext,
            userManager,
            Options.Create(new ContactOptions()));
        var response = await service.GetEligibilityAsync(user.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.False(response.IsEmailVerified);
        Assert.True(response.IsAccountActive);
        Assert.False(response.IsProfileComplete);
        Assert.False(response.HasUploadedProfileImage);
        Assert.False(response.HasCompletedRequiredQuestions);
        Assert.True(response.DiscoverabilityOptIn);
        Assert.True(response.HasRecipientCapacity);
        Assert.True(response.HasNoActiveModerationRestriction);
        Assert.False(response.IsEligibleForDiscoverability);
    }

    [Fact]
    public async Task WhenPreviouslyOptionalQuestionIsUnansweredThenMemberIsIneligible()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await CreateEligibleMemberAsync(dbContext, userManager, validAnswer: true);
        dbContext.Questions.Add(new Question
        {
            Id = Guid.NewGuid(), Code = $"required-{Guid.NewGuid():N}", Type = QuestionType.Scale,
            Category = "interests", ScaleMin = 1, ScaleMax = 5, ScoringWeight = 1
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = new AccountEligibilityService(dbContext, userManager, Options.Create(new ContactOptions()));

        var result = await service.GetEligibilityAsync(user.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.False(result.HasCompletedRequiredQuestions);
    }

    [Fact]
    public async Task WhenMemberIsEligibleThenRecommendationAvailabilityIsTrue()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
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
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
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
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await CreateEligibleMemberAsync(dbContext, userManager, validAnswer: true);
        var service = new AccountEligibilityService(dbContext, userManager, Options.Create(new ContactOptions()));

        var eligibleResponse = await service.GetEligibilityAsync(user.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(eligibleResponse);
        Assert.True(eligibleResponse.IsEligibleForDiscoverability);

        var lifecycle = await dbContext.AccountLifecycles.SingleAsync(item => item.ApplicationUserId == user.Id, TestContext.Current.CancellationToken);
        lifecycle.DiscoverabilityOptIn = false;
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var optedOutResponse = await service.GetEligibilityAsync(user.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(optedOutResponse);
        Assert.False(optedOutResponse.DiscoverabilityOptIn);
        Assert.False(optedOutResponse.IsEligibleForDiscoverability);
    }

    [Fact]
    public async Task WhenRequiredAnswerHasAnInvalidStoredShapeThenMemberIsIneligible()
    {
        using var services = CreateServices();
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await CreateEligibleMemberAsync(dbContext, userManager, validAnswer: false);
        var service = new AccountEligibilityService(dbContext, userManager, Options.Create(new ContactOptions()));

        var response = await service.GetEligibilityAsync(user.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.False(response.HasCompletedRequiredQuestions);
        Assert.False(response.IsEligibleForDiscoverability);
    }

    private ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(fixture.ConnectionString));
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
            Category = "interests",
            ScoringWeight = 1,
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
