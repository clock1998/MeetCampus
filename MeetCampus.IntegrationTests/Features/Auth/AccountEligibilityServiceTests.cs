using MeetCampus.Data;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.User;
using MeetCampus.Features.Auth.Services;
using MeetCampus.Features.Configuration;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

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
        dbContext.QuestionDefinitions.Add(new Question
        {
            Id = Guid.NewGuid(),
            Code = $"required-{Guid.NewGuid():N}",
            Type = QuestionType.SingleChoice,
            PromptResourceKey = "Eligibility.Test.Prompt",
            IsRequired = true,
            Group = "eligibility",
            QuestionVisibility = QuestionVisibility.Shareable,
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
}
