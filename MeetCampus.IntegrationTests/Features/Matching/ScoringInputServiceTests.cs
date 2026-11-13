using MeetCampus.Contracts.Features.Administration;
using MeetCampus.Data;
using MeetCampus.Data.Profiles;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.User;
using MeetCampus.Features.Matching.Services;
using MeetCampus.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeetCampus.IntegrationTests.Features.Matching;

public sealed class ScoringInputServiceTests(PostgreSqlContainerFixture fixture) : IClassFixture<PostgreSqlContainerFixture>
{
    [Fact]
    public async Task WhenBothMembersHaveCurrentAnswersThenEachAnswerIsLoadedForItsOwner()
    {
        await using var db = await CreateContextAsync();
        var first = AddUser(db);
        var second = AddUser(db);
        var other = AddUser(db);
        var question = AddQuestion(db, QuestionType.SingleChoice);
        AddAnswer(db, first, question, question.Options.First().Id);
        AddAnswer(db, second, question, question.Options.Last().Id);
        AddAnswer(db, other, question, question.Options.First().Id);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var input = await new ScoringInputService(db).LoadAsync(first.Id, second.Id, TestContext.Current.CancellationToken);
        var result = input.Questions.Single(item => item.Id == question.Id);

        Assert.Equal([question.Options.First().Id], result.FirstAnswer!.OptionIds);
        Assert.Equal([question.Options.Last().Id], result.SecondAnswer!.OptionIds);
    }

    [Fact]
    public async Task WhenAnswerIsMissingThenQuestionRemainsWithoutAnAnswer()
    {
        await using var db = await CreateContextAsync();
        var first = AddUser(db);
        var second = AddUser(db);
        var question = AddQuestion(db, QuestionType.SingleChoice);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var input = await new ScoringInputService(db).LoadAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Null(input.Questions.Single(item => item.Id == question.Id).FirstAnswer);
    }

    [Fact]
    public async Task WhenAnswerIsOlderThanQuestionThenItIsNotUsable()
    {
        await using var db = await CreateContextAsync();
        var first = AddUser(db);
        var second = AddUser(db);
        var question = AddQuestion(db, QuestionType.SingleChoice);
        var answer = AddAnswer(db, first, question, question.Options.First().Id);
        answer.UpdatedAt = question.UpdatedAt.AddMinutes(-1);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var input = await new ScoringInputService(db).LoadAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Null(input.Questions.Single(item => item.Id == question.Id).FirstAnswer);
    }

    [Fact]
    public async Task WhenAnswerUsesAnotherQuestionsOptionThenItIsNotUsable()
    {
        await using var db = await CreateContextAsync();
        var first = AddUser(db);
        var second = AddUser(db);
        var question = AddQuestion(db, QuestionType.SingleChoice);
        var otherQuestion = AddQuestion(db, QuestionType.SingleChoice);
        AddAnswer(db, first, question, otherQuestion.Options.First().Id);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var input = await new ScoringInputService(db).LoadAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Null(input.Questions.Single(item => item.Id == question.Id).FirstAnswer);
    }

    [Fact]
    public async Task WhenMultipleChoiceContainsDuplicateOptionsThenItIsNotUsable()
    {
        await using var db = await CreateContextAsync();
        var first = AddUser(db);
        var second = AddUser(db);
        var question = AddQuestion(db, QuestionType.MultipleChoice);
        AddAnswer(db, first, question, question.Options.First().Id);
        AddAnswer(db, first, question, question.Options.First().Id);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var input = await new ScoringInputService(db).LoadAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Null(input.Questions.Single(item => item.Id == question.Id).FirstAnswer);
    }

    [Fact]
    public async Task WhenMultipleChoicesAreCurrentThenOptionIdsAreSorted()
    {
        await using var db = await CreateContextAsync();
        var first = AddUser(db);
        var second = AddUser(db);
        var question = AddQuestion(db, QuestionType.MultipleChoice);
        foreach (var option in question.Options.OrderByDescending(option => option.Id))
            AddAnswer(db, first, question, option.Id);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var input = await new ScoringInputService(db).LoadAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Equal(question.Options.Select(option => option.Id).Order(), input.Questions.Single(item => item.Id == question.Id).FirstAnswer!.OptionIds);
    }

    [Fact]
    public async Task WhenScaleAnswerIsCurrentThenItsValueIsLoaded()
    {
        await using var db = await CreateContextAsync();
        var first = AddUser(db);
        var second = AddUser(db);
        var question = AddQuestion(db, QuestionType.Scale);
        db.UserAnswers.Add(new UserAnswer { Id = Guid.NewGuid(), ApplicationUserId = first.Id, QuestionId = question.Id, ScaleValue = 3, UpdatedAt = question.UpdatedAt });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var input = await new ScoringInputService(db).LoadAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Equal(3, input.Questions.Single(item => item.Id == question.Id).FirstAnswer!.ScaleValue);
    }

    [Fact]
    public async Task WhenQuestionHasUnsupportedScaleThenItIsNotLoaded()
    {
        await using var db = await CreateContextAsync();
        var first = AddUser(db);
        var second = AddUser(db);
        var question = AddQuestion(db, QuestionType.Scale);
        question.ScaleMax = question.ScaleMin;
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var input = await new ScoringInputService(db).LoadAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.DoesNotContain(input.Questions, item => item.Id == question.Id);
    }

    [Fact]
    public async Task WhenMembersHaveIntentionsThenEachMembersIntentionsAreLoadedInOrder()
    {
        await using var db = await CreateContextAsync();
        var first = AddUser(db);
        var second = AddUser(db);
        var firstProfile = AddProfile(db, first);
        var secondProfile = AddProfile(db, second);
        var firstIntent = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var secondIntent = Guid.Parse("30000000-0000-0000-0000-000000000002");
        db.UserProfileIntentions.AddRange(
            new UserProfileIntention { UserProfileId = firstProfile.Id, UserIntentionId = secondIntent },
            new UserProfileIntention { UserProfileId = firstProfile.Id, UserIntentionId = firstIntent },
            new UserProfileIntention { UserProfileId = secondProfile.Id, UserIntentionId = secondIntent });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var input = await new ScoringInputService(db).LoadAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { firstIntent, secondIntent }.Order(), input.FirstIntentIds);
        Assert.Equal([secondIntent], input.SecondIntentIds);
    }

    [Fact]
    public async Task WhenQuestionsAreLoadedThenTheyAreSortedById()
    {
        await using var db = await CreateContextAsync();
        var first = AddUser(db);
        var second = AddUser(db);
        AddQuestion(db, QuestionType.SingleChoice);
        AddQuestion(db, QuestionType.Scale);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var input = await new ScoringInputService(db).LoadAsync(first.Id, second.Id, TestContext.Current.CancellationToken);

        Assert.Equal(input.Questions.Select(question => question.Id).Order(), input.Questions.Select(question => question.Id));
    }

    private async Task<ApplicationDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(fixture.ConnectionString).Options;
        var db = new ApplicationDbContext(options);
        await db.Database.MigrateAsync(TestContext.Current.CancellationToken);
        return db;
    }

    private static ApplicationUser AddUser(ApplicationDbContext db)
    {
        var id = Guid.NewGuid();
        var user = new ApplicationUser { Id = id, UserName = $"scoring-{id:N}@example.edu", SecurityStamp = Guid.NewGuid().ToString() };
        db.Users.Add(user);
        return user;
    }

    private static UserProfile AddProfile(ApplicationDbContext db, ApplicationUser user)
    {
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = user.Id,
            StudyDomainId = Guid.Parse("10000000-0000-0000-0000-000000000011"),
            SchoolId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            LanguageId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
            EthnicityId = Guid.Parse("40000000-0000-0000-0000-000000000001")
        };
        db.UserProfiles.Add(profile);
        return profile;
    }

    private static Question AddQuestion(ApplicationDbContext db, QuestionType type)
    {
        var question = new Question
        {
            Id = Guid.NewGuid(),
            Code = $"scoring-{Guid.NewGuid():N}",
            CategoryId = Guid.Parse("80000000-0000-0000-0000-000000000001"),
            Type = type,
            ScoringWeight = 0.5m,
            UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            ScaleMin = type == QuestionType.Scale ? 1 : null,
            ScaleMax = type == QuestionType.Scale ? 5 : null,
            Options = type == QuestionType.Scale ? [] :
            [
                new QuestionOption { Id = Guid.NewGuid(), OptionCode = "first", SortOrder = 0 },
                new QuestionOption { Id = Guid.NewGuid(), OptionCode = "second", SortOrder = 1 }
            ]
        };
        db.Questions.Add(question);
        return question;
    }

    private static UserAnswer AddAnswer(ApplicationDbContext db, ApplicationUser user, Question question, Guid optionId)
    {
        var answer = new UserAnswer
        {
            Id = Guid.NewGuid(), ApplicationUserId = user.Id, QuestionId = question.Id,
            QuestionOptionId = optionId, UpdatedAt = question.UpdatedAt
        };
        db.UserAnswers.Add(answer);
        return answer;
    }
}
