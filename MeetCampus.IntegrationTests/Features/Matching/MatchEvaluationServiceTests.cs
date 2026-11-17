using MeetCampus.Configuration;
using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Contracts.Features.Auth;
using MeetCampus.Data;
using MeetCampus.Data.Matching;
using MeetCampus.Data.Profiles;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.Safety;
using MeetCampus.Data.User;
using MeetCampus.Features.Account.Services;
using MeetCampus.Features.Matching.Models;
using MeetCampus.Features.Matching.Services;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Xunit;

namespace MeetCampus.IntegrationTests.Features.Matching;

public sealed class MatchEvaluationServiceTests(PostgreSqlContainerFixture fixture) : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task WhenLoadedUsersHaveCompleteAnswersThenTheyAreEligibleForMatching()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (first, second, question) = await CreatePairAsync(scope.ServiceProvider);
        AddAnswer(db, first, question);
        AddAnswer(db, second, question);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var account = new AccountEligibilityService(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), Options.Create(new ContactOptions()));
        var matching = new MatchEligibilityService(db, account);

        var result = await matching.IsEligibleForMatchingAsync(first, second, TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    [Fact]
    public async Task WhenLoadedUsersAreBlockedThenTheyAreNotEligibleForMatching()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (first, second, question) = await CreatePairAsync(scope.ServiceProvider);
        AddAnswer(db, first, question);
        AddAnswer(db, second, question);
        db.UserBlocks.Add(new UserBlock { Id = Guid.NewGuid(), BlockerUserId = first.Id, BlockedUserId = second.Id, CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var account = new AccountEligibilityService(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), Options.Create(new ContactOptions()));
        var matching = new MatchEligibilityService(db, account);

        var result = await matching.IsEligibleForMatchingAsync(first, second, TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenEitherAccountDoesNotExistThenPairIsIneligible(bool firstIsMissing)
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var (first, _, _) = await CreatePairAsync(scope.ServiceProvider);
        var missingId = Guid.NewGuid();

        var result = await CreateEvaluator(scope.ServiceProvider).EvaluateAsync(
            firstIsMissing ? missingId : first.Id,
            firstIsMissing ? first.Id : missingId,
            TestContext.Current.CancellationToken);

        Assert.Equal(new MatchEvaluationResult(MatchEvaluationStatus.Ineligible, null, null), result);
    }

    [Fact]
    public async Task WhenAllAccountRequirementsAreMetThenResponseHasNoFailures()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var (first, _, question) = await CreatePairAsync(scope.ServiceProvider);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        AddAnswer(db, first, question);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var account = new AccountEligibilityService(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), Options.Create(new ContactOptions()));

        var eligibility = await account.GetEligibilityAsync(first, TestContext.Current.CancellationToken);

        Assert.Equal(AccountEligibilityFailure.None, Assert.IsType<AccountEligibilityResponse>(eligibility).Failures);
    }

    [Fact]
    public async Task WhenEmailIsUnverifiedAndAnswersAreMissingThenResponseContainsBothFailures()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var (first, _, _) = await CreatePairAsync(scope.ServiceProvider);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        first.EmailConfirmed = false;
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var account = new AccountEligibilityService(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), Options.Create(new ContactOptions()));

        var eligibility = await account.GetEligibilityAsync(first, TestContext.Current.CancellationToken);

        Assert.Equal(AccountEligibilityFailure.EmailNotVerified | AccountEligibilityFailure.AnswersIncomplete,
            Assert.IsType<AccountEligibilityResponse>(eligibility).Failures);
    }

    [Fact]
    public async Task WhenAnswersAreMissingAndMemberOptsOutThenAccountReportsBothFailures()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var (first, _, _) = await CreatePairAsync(scope.ServiceProvider);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var lifecycle = await db.AccountLifecycles.SingleAsync(item => item.ApplicationUserId == first.Id, TestContext.Current.CancellationToken);
        lifecycle.DiscoverabilityOptIn = false;
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var account = new AccountEligibilityService(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), Options.Create(new ContactOptions()));

        var eligibility = await account.GetEligibilityAsync(first, TestContext.Current.CancellationToken);

        Assert.Equal(AccountEligibilityFailure.AnswersIncomplete | AccountEligibilityFailure.DiscoverabilityNotOptedIn,
            Assert.IsType<AccountEligibilityResponse>(eligibility).Failures);
    }

    [Fact]
    public async Task WhenAnswersAreMissingThenPublicAnswerCheckReturnsFalse()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var (first, _, _) = await CreatePairAsync(scope.ServiceProvider);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var account = new AccountEligibilityService(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), Options.Create(new ContactOptions()));

        var result = await account.HasCompletedRequiredQuestionsAsync(first.Id, TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    [Fact]
    public async Task WhenProfileHasAnUploadedImageThenPublicImageCheckReturnsTrue()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var (first, _, _) = await CreatePairAsync(scope.ServiceProvider);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var account = new AccountEligibilityService(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), Options.Create(new ContactOptions()));
        var profile = await account.GetProfileAsync(first.Id, TestContext.Current.CancellationToken);

        var result = await account.HasUploadedProfileImageAsync(profile, TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    [Fact]
    public async Task WhenAccountHasNoInboundRequestsThenPublicCapacityCheckReturnsTrue()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var (first, _, _) = await CreatePairAsync(scope.ServiceProvider);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var account = new AccountEligibilityService(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), Options.Create(new ContactOptions()));

        var result = await account.HasRecipientCapacityAsync(first.Id, DateTimeOffset.UtcNow, TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    [Fact]
    public async Task WhenAccountHasNoModerationActionsThenPublicRestrictionCheckReturnsTrue()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var (first, _, _) = await CreatePairAsync(scope.ServiceProvider);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var account = new AccountEligibilityService(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), Options.Create(new ContactOptions()));

        var result = await account.HasNoActiveModerationRestrictionAsync(first.Id, DateTimeOffset.UtcNow, TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    [Theory]
    [InlineData(false, true, MatchEvaluationStatus.FirstMemberAnswersIncomplete)]
    [InlineData(true, false, MatchEvaluationStatus.SecondMemberAnswersIncomplete)]
    [InlineData(false, false, MatchEvaluationStatus.BothMembersAnswersIncomplete)]
    public async Task WhenAnswersAreMissingThenEvaluationReportsTheAffectedMembers(bool firstAnswered, bool secondAnswered, MatchEvaluationStatus expected)
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (first, second, question) = await CreatePairAsync(scope.ServiceProvider);
        if (firstAnswered) AddAnswer(db, first, question);
        if (secondAnswered) AddAnswer(db, second, question);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateEvaluator(scope.ServiceProvider).EvaluateAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Equal(expected, result.Status);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenEitherMemberBlocksAnIncompletePairThenEvaluationIsIneligible(bool firstBlocksSecond)
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (first, second, _) = await CreatePairAsync(scope.ServiceProvider);
        db.UserBlocks.Add(new UserBlock { Id = Guid.NewGuid(), BlockerUserId = firstBlocksSecond ? first.Id : second.Id, BlockedUserId = firstBlocksSecond ? second.Id : first.Id, CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateEvaluator(scope.ServiceProvider).EvaluateAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Equal(new MatchEvaluationResult(MatchEvaluationStatus.Ineligible, null, null), result);
    }

    [Fact]
    public async Task WhenIncompleteMemberOptsOutThenEvaluationIsIneligible()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (first, second, _) = await CreatePairAsync(scope.ServiceProvider);
        var lifecycle = await db.AccountLifecycles.SingleAsync(item => item.ApplicationUserId == second.Id, TestContext.Current.CancellationToken);
        lifecycle.DiscoverabilityOptIn = false;
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateEvaluator(scope.ServiceProvider).EvaluateAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Equal(MatchEvaluationStatus.Ineligible, result.Status);
    }

    [Fact]
    public async Task WhenIncompleteMembersHaveDifferentIntentsThenEvaluationIsIneligible()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (first, second, _) = await CreatePairAsync(scope.ServiceProvider, "30000000-0000-0000-0000-000000000002");

        var result = await CreateEvaluator(scope.ServiceProvider).EvaluateAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Equal(MatchEvaluationStatus.Ineligible, result.Status);
    }

    [Fact]
    public async Task WhenBothMembersHaveCurrentAnswersThenEvaluationReturnsScoreAndSelectedCategory()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (first, second, question) = await CreatePairAsync(scope.ServiceProvider);
        AddAnswer(db, first, question);
        AddAnswer(db, second, question);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateEvaluator(scope.ServiceProvider).EvaluateAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Equal((MatchEvaluationStatus.Completed, 100m, 1m, question.CategoryId),
            (result.Status, result.Score?.Score, result.Score?.EvidenceCoverage, Assert.Single(result.Explanation!.Categories).Id));
    }

    [Fact]
    public async Task WhenAnAnswerIsOutdatedThenEvaluationSkipsScoring()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (first, second, question) = await CreatePairAsync(scope.ServiceProvider);
        AddAnswer(db, first, question).UpdatedAt = question.UpdatedAt.AddMinutes(-1);
        AddAnswer(db, second, question);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateEvaluator(scope.ServiceProvider).EvaluateAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Equal(new MatchEvaluationResult(MatchEvaluationStatus.FirstMemberAnswersIncomplete, null, null), result);
    }

    [Fact]
    public async Task WhenPopulatedCategoryHasNoWeightThenScoreIsUnavailable()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (first, second, question) = await CreatePairAsync(scope.ServiceProvider);
        var category = await db.QuestionCategories.SingleAsync(item => item.Id == question.CategoryId, TestContext.Current.CancellationToken);
        category.MatchingWeight = 0;
        AddAnswer(db, first, question);
        AddAnswer(db, second, question);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateEvaluator(scope.ServiceProvider).EvaluateAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Equal(MatchEvaluationStatus.ScoreUnavailable, result.Status);
    }

    [Fact]
    public async Task WhenCategoryIsRenamedThenStoredReferenceUsesCurrentName()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (first, second, _) = await CreatePairAsync(scope.ServiceProvider);
        var category = new QuestionCategory { Id = Guid.NewGuid(), Code = "selected", NameEn = "Original", NameFr = "Original", SortOrder = 100, MatchingWeight = 0 };
        var item = AddRecommendation(db, first, second, category);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        category.NameEn = "Updated";
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        db.ChangeTracker.Clear();

        var name = await db.RecommendationExplanationCategories.Where(reference => reference.RecommendationItemId == item.Id)
            .Select(reference => reference.QuestionCategory.NameEn).SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal("Updated", name);
    }

    [Fact]
    public async Task WhenUnusedCategoryIsDeletedThenRecommendationRemainsWithoutItsReference()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (first, second, _) = await CreatePairAsync(scope.ServiceProvider);
        var category = new QuestionCategory { Id = Guid.NewGuid(), Code = "selected", NameEn = "Selected", NameFr = "Selected", SortOrder = 100, MatchingWeight = 0 };
        var item = AddRecommendation(db, first, second, category);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        await db.QuestionCategories.Where(entry => entry.Id == category.Id).ExecuteDeleteAsync(TestContext.Current.CancellationToken);
        db.ChangeTracker.Clear();

        var stored = await db.RecommendationItems.Include(entry => entry.ExplanationCategories).SingleAsync(entry => entry.Id == item.Id, TestContext.Current.CancellationToken);

        Assert.Empty(stored.ExplanationCategories);
    }

    private async Task<ServiceProvider> CreateServicesAsync()
    {
        var databaseName = $"evaluation_{Guid.NewGuid():N}";
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection);
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        var connectionString = new NpgsqlConnectionStringBuilder(fixture.ConnectionString) { Database = databaseName }.ConnectionString;
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<ApplicationDbContext>();
        var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        return provider;
    }

    private static MatchEvaluationService CreateEvaluator(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var account = new AccountEligibilityService(db, services.GetRequiredService<UserManager<ApplicationUser>>(), Options.Create(new ContactOptions()));
        return new MatchEvaluationService(new MatchEligibilityService(db, account), new ScoringInputService(db));
    }

    private static async Task<(ApplicationUser First, ApplicationUser Second, Question Question)> CreatePairAsync(IServiceProvider services, string secondIntent = "30000000-0000-0000-0000-000000000001")
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var manager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var first = await CreateUserAsync(db, manager, "30000000-0000-0000-0000-000000000001");
        var second = await CreateUserAsync(db, manager, secondIntent);
        var question = new Question
        {
            Id = Guid.NewGuid(), Code = "evaluation", CategoryId = Guid.Parse("80000000-0000-0000-0000-000000000001"),
            Type = QuestionType.SingleChoice, ScoringWeight = 0.5m, UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            Options = [new QuestionOption { Id = Guid.NewGuid(), OptionCode = "yes", SortOrder = 0 }]
        };
        db.Questions.Add(question);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (first, second, question);
    }

    private static async Task<ApplicationUser> CreateUserAsync(ApplicationDbContext db, UserManager<ApplicationUser> manager, string intent)
    {
        var email = $"evaluation-{Guid.NewGuid():N}@example.edu";
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        Assert.True((await manager.CreateAsync(user, "Test-password1!")).Succeeded);
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(), ApplicationUserId = user.Id,
            StudyDomainId = Guid.Parse("10000000-0000-0000-0000-000000000011"), SchoolId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            LanguageId = Guid.Parse("70000000-0000-0000-0000-000000000001"), EthnicityId = Guid.Parse("40000000-0000-0000-0000-000000000001")
        };
        db.UserProfiles.Add(profile);
        db.UserProfileIntentions.Add(new UserProfileIntention { UserProfileId = profile.Id, UserIntentionId = Guid.Parse(intent) });
        db.UserProfileImages.Add(new UserProfileImage { Id = Guid.NewGuid(), UserProfileId = profile.Id, ImageKey = $"evaluation/{user.Id:N}.webp", IsAvatar = true });
        db.AccountLifecycles.Add(new AccountLifecycle { ApplicationUserId = user.Id, Status = AccountStatus.Active, DiscoverabilityOptIn = true });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return user;
    }

    private static UserAnswer AddAnswer(ApplicationDbContext db, ApplicationUser user, Question question)
    {
        var answer = new UserAnswer { Id = Guid.NewGuid(), ApplicationUserId = user.Id, QuestionId = question.Id, QuestionOptionId = question.Options.Single().Id, UpdatedAt = question.UpdatedAt };
        db.UserAnswers.Add(answer);
        return answer;
    }

    private static RecommendationItem AddRecommendation(ApplicationDbContext db, ApplicationUser first, ApplicationUser second, QuestionCategory category)
    {
        var batch = new RecommendationBatch { Id = Guid.NewGuid(), ViewerUserId = first.Id, CreatedAt = DateTimeOffset.UtcNow, BatchDate = DateOnly.FromDateTime(DateTime.UtcNow) };
        var item = new RecommendationItem { Id = Guid.NewGuid(), RecommendationBatch = batch, CandidateUserId = second.Id, CompatibilityScore = 100m, EvidenceCoverage = 1m };
        item.ExplanationCategories.Add(new RecommendationExplanationCategory { RecommendationItem = item, QuestionCategory = category, SortOrder = 0 });
        db.RecommendationItems.Add(item);
        return item;
    }
}
