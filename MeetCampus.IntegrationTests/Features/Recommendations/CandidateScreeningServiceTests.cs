using MeetCampus.Configuration;
using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Data;
using MeetCampus.Data.Contact;
using MeetCampus.Data.Matching;
using MeetCampus.Data.Profiles;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.Safety;
using MeetCampus.Data.User;
using MeetCampus.Features.Account.Services;
using MeetCampus.Features.Matching.Services;
using MeetCampus.Features.Recommendations.Services;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using Xunit;

namespace MeetCampus.IntegrationTests.Features.Recommendations;

public sealed class CandidateScreeningServiceTests(PostgreSqlContainerFixture fixture) : IClassFixture<PostgreSqlContainerFixture>
{
    private static readonly Guid FriendshipIntentId = Guid.Parse("30000000-0000-0000-0000-000000000001");

    [Theory]
    [InlineData(ContactState.Pending, false)]
    [InlineData(ContactState.Accepted, true)]
    [InlineData(ContactState.Expired, false)]
    [InlineData(ContactState.Cancelled, true)]
    [InlineData(ContactState.Blocked, false)]
    public async Task WhenContactHistoryExistsInEitherDirectionThenCandidateIsExcluded(ContactState state, bool candidateSentRequest)
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (viewer, question) = await CreateUserAsync(scope.ServiceProvider);
        var (candidate, _) = await CreateUserAsync(scope.ServiceProvider);
        AddAnswer(db, viewer, question, "yes");
        AddAnswer(db, candidate, question, "yes");
        db.ContactRequests.Add(new ContactRequest
        {
            Id = Guid.NewGuid(),
            SenderUserId = candidateSentRequest ? candidate.Id : viewer.Id,
            RecipientUserId = candidateSentRequest ? viewer.Id : candidate.Id,
            SenderIntentId = FriendshipIntentId,
            RecipientIntentId = FriendshipIntentId,
            State = state,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateScreeningService(scope.ServiceProvider).ScreenAsync(viewer, TestContext.Current.CancellationToken);

        Assert.True(result.ViewerIsEligible);
        Assert.Empty(result.Candidates);
    }

    [Fact]
    public async Task WhenCandidateHasExistingItemsThenOnlyExpiredPassIsEligibleToBeScreenedAgain()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (viewer, question) = await CreateUserAsync(scope.ServiceProvider);
        var unseen = await CreateUserAsync(scope.ServiceProvider);
        var saved = await CreateUserAsync(scope.ServiceProvider);
        var recentPass = await CreateUserAsync(scope.ServiceProvider);
        var oldPass = await CreateUserAsync(scope.ServiceProvider);
        var missingPassTime = await CreateUserAsync(scope.ServiceProvider);
        var candidates = new List<(ApplicationUser User, RecommendationDecision Decision, DateTimeOffset? DecidedAt)>
        {
            (unseen.User, RecommendationDecision.Unseen, null),
            (saved.User, RecommendationDecision.Saved, null),
            (recentPass.User, RecommendationDecision.Passed, DateTimeOffset.UtcNow.AddDays(-89)),
            (oldPass.User, RecommendationDecision.Passed, DateTimeOffset.UtcNow.AddDays(-91)),
            (missingPassTime.User, RecommendationDecision.Passed, null)
        };
        AddAnswer(db, viewer, question, "yes");
        foreach (var (candidate, _, _) in candidates)
            AddAnswer(db, candidate, question, "yes");

        var batch = new RecommendationBatch
        {
            Id = Guid.NewGuid(),
            ViewerUserId = viewer.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            BatchDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        foreach (var (candidate, decision, decidedAt) in candidates)
        {
            batch.Items.Add(new RecommendationItem
            {
                Id = Guid.NewGuid(),
                CandidateUserId = candidate.Id,
                Decision = decision,
                DecidedAt = decidedAt,
                CompatibilityScore = 100m,
                EvidenceCoverage = 1m
            });
        }
        db.RecommendationBatches.Add(batch);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateScreeningService(scope.ServiceProvider).ScreenAsync(viewer, TestContext.Current.CancellationToken);

        Assert.True(result.ViewerIsEligible);
        Assert.Equal([candidates[3].User.Id], result.Candidates.Select(candidate => candidate.CandidateUserId));
    }

    [Fact]
    public async Task WhenViewerIsEligibleThenOnlyCompleteCandidatesMeetingThresholdAreReturnedInStableOrder()
    {
        using var services = await CreateServicesAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (viewer, question) = await CreateUserAsync(scope.ServiceProvider);
        var (matchingCandidate, _) = await CreateUserAsync(scope.ServiceProvider, Guid.Parse("90000000-0000-0000-0000-000000000002"));
        var (matchingCandidateWithLowerId, _) = await CreateUserAsync(scope.ServiceProvider, Guid.Parse("90000000-0000-0000-0000-000000000001"));
        var (lowScoreCandidate, _) = await CreateUserAsync(scope.ServiceProvider);
        var (incompleteCandidate, _) = await CreateUserAsync(scope.ServiceProvider);
        AddAnswer(db, viewer, question, "yes");
        AddAnswer(db, matchingCandidate, question, "yes");
        AddAnswer(db, matchingCandidateWithLowerId, question, "yes");
        AddAnswer(db, lowScoreCandidate, question, "no");
        var lifecycle = await db.AccountLifecycles.SingleAsync(item => item.ApplicationUserId == viewer.Id, TestContext.Current.CancellationToken);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = CreateScreeningService(scope.ServiceProvider);

        var eligibleResult = await service.ScreenAsync(viewer, TestContext.Current.CancellationToken);

        Assert.True(eligibleResult.ViewerIsEligible);
        Assert.Equal([matchingCandidateWithLowerId.Id, matchingCandidate.Id], eligibleResult.Candidates.Select(candidate => candidate.CandidateUserId));
        Assert.All(eligibleResult.Candidates, candidate => Assert.Equal(100m, candidate.Score.Score));

        lifecycle.DiscoverabilityOptIn = false;
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var ineligibleResult = await service.ScreenAsync(viewer, TestContext.Current.CancellationToken);

        Assert.False(ineligibleResult.ViewerIsEligible);
        Assert.Empty(ineligibleResult.Candidates);
    }

    private async Task<ServiceProvider> CreateServicesAsync()
    {
        var databaseName = $"screening_{Guid.NewGuid():N}";
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

    private static CandidateScreeningService CreateScreeningService(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var account = new AccountEligibilityService(db, services.GetRequiredService<UserManager<ApplicationUser>>(), Options.Create(new ContactOptions()));
        var evaluator = new MatchEvaluationService(new MatchEligibilityService(db, account), new ScoringInputService(db));
        return new CandidateScreeningService(
            db,
            account,
            evaluator,
            Options.Create(new MatchingOptions { MinimumCompatibilityScore = 100 }),
            NullLogger<CandidateScreeningService>.Instance);
    }

    private static async Task<(ApplicationUser User, Question Question)> CreateUserAsync(IServiceProvider services, Guid? userId = null)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var manager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"screening-{Guid.NewGuid():N}@example.edu";
        var user = new ApplicationUser { Id = userId ?? Guid.NewGuid(), UserName = email, Email = email, EmailConfirmed = true };
        Assert.True((await manager.CreateAsync(user, "Test-password1!")).Succeeded);
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(), ApplicationUserId = user.Id,
            StudyDomainId = Guid.Parse("10000000-0000-0000-0000-000000000011"), SchoolId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            LanguageId = Guid.Parse("70000000-0000-0000-0000-000000000001"), EthnicityId = Guid.Parse("40000000-0000-0000-0000-000000000001")
        };
        db.UserProfiles.Add(profile);
        db.UserProfileIntentions.Add(new UserProfileIntention { UserProfileId = profile.Id, UserIntentionId = FriendshipIntentId });
        db.UserProfileImages.Add(new UserProfileImage { Id = Guid.NewGuid(), UserProfileId = profile.Id, ImageKey = $"screening/{user.Id:N}.webp", IsAvatar = true });
        db.AccountLifecycles.Add(new AccountLifecycle { ApplicationUserId = user.Id, Status = AccountStatus.Active, DiscoverabilityOptIn = true });
        var question = await db.Questions.AsNoTracking().Include(item => item.Options)
            .SingleOrDefaultAsync(item => item.Code == "candidate-screening", TestContext.Current.CancellationToken);
        if (question is null)
        {
            question = new Question
            {
                Id = Guid.NewGuid(), Code = "candidate-screening", CategoryId = Guid.Parse("80000000-0000-0000-0000-000000000001"),
                Type = QuestionType.SingleChoice, ScoringWeight = 0.5m, UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                Options =
                [
                    new QuestionOption { Id = Guid.NewGuid(), OptionCode = "yes", SortOrder = 0 },
                    new QuestionOption { Id = Guid.NewGuid(), OptionCode = "no", SortOrder = 1 }
                ]
            };
            db.Questions.Add(question);
        }
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (user, question);
    }

    private static void AddAnswer(ApplicationDbContext db, ApplicationUser user, Question question, string optionCode)
    {
        var option = question.Options.Single(item => item.OptionCode == optionCode);
        db.UserAnswers.Add(new UserAnswer
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = user.Id,
            QuestionId = question.Id,
            QuestionOptionId = option.Id,
            UpdatedAt = question.UpdatedAt
        });
    }
}