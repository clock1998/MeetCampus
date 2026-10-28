using System.Security.Claims;
using System.Text.Json;
using MeetCampus.Data;
using MeetCampus.Data.Audit;
using MeetCampus.Data.Matching;
using MeetCampus.Data.Questionnaire;
using MeetCampus.Data.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Features.Administration.Services;

public sealed class AdministrativeDataService(
    ApplicationDbContext dbContext,
    IAuthorizationService authorizationService)
{
    public async Task SetAllowedEmailDomainAsync(
        ClaimsPrincipal actor,
        string domain,
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        var actorId = await RequireAdministratorAsync(actor);
        var normalizedDomain = NormalizeDomain(domain);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var allowedDomain = await dbContext.AllowedEmailDomains
            .SingleOrDefaultAsync(item => item.Domain == normalizedDomain, cancellationToken);

        if (allowedDomain is null)
        {
            allowedDomain = new AllowedEmailDomain
            {
                Id = Guid.NewGuid(),
                Domain = normalizedDomain,
                CreatedAt = DateTimeOffset.UtcNow
            };
            dbContext.AllowedEmailDomains.Add(allowedDomain);
        }

        allowedDomain.IsEnabled = isEnabled;
        dbContext.AuditEvents.Add(CreateAuditEvent(actorId, "EmailDomain.SetEnabled", "AllowedEmailDomain", allowedDomain.Id.ToString(), new { normalizedDomain, isEnabled }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task SetIntentCompatibilityAsync(
        ClaimsPrincipal actor,
        Guid senderIntentId,
        Guid recipientIntentId,
        bool isAllowed,
        CancellationToken cancellationToken = default)
    {
        var actorId = await RequireAdministratorAsync(actor);
        if (senderIntentId == recipientIntentId)
            throw new ArgumentException("Sender and recipient intents must differ.", nameof(recipientIntentId));

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var rule = await dbContext.IntentCompatibilityRules.SingleOrDefaultAsync(
            item => item.SenderIntentId == senderIntentId && item.RecipientIntentId == recipientIntentId,
            cancellationToken);

        if (rule is null)
        {
            rule = new IntentCompatibilityRule
            {
                Id = Guid.NewGuid(),
                SenderIntentId = senderIntentId,
                RecipientIntentId = recipientIntentId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            dbContext.IntentCompatibilityRules.Add(rule);
        }

        rule.IsAllowed = isAllowed;
        dbContext.AuditEvents.Add(CreateAuditEvent(actorId, "IntentCompatibility.Set", "IntentCompatibilityRule", rule.Id.ToString(), new { senderIntentId, recipientIntentId, isAllowed }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<Question> CreateQuestionAsync(
        ClaimsPrincipal actor,
        Question question,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(question);
        var actorId = await RequireAdministratorAsync(actor);
        ValidateQuestion(question);
        question.Id = question.Id == Guid.Empty ? Guid.NewGuid() : question.Id;
        question.UpdatedAt = DateTimeOffset.UtcNow;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.QuestionDefinitions.Add(question);
        dbContext.AuditEvents.Add(CreateAuditEvent(actorId, "Question.Created", "QuestionDefinition", question.Id.ToString(), new { question.Code }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return question;
    }

    public async Task UpdateQuestionAsync(
        ClaimsPrincipal actor,
        Guid questionId,
        Question updatedQuestion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updatedQuestion);
        var actorId = await RequireAdministratorAsync(actor);
        ValidateQuestion(updatedQuestion);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var question = await dbContext.QuestionDefinitions
            .Include(item => item.Options)
            .SingleOrDefaultAsync(item => item.Id == questionId, cancellationToken)
            ?? throw new KeyNotFoundException("Question was not found.");

        if (!string.Equals(question.Code, updatedQuestion.Code, StringComparison.Ordinal))
            throw new InvalidOperationException("A question's stable ID cannot be changed because stored answers refer to it.");

        question.Code = updatedQuestion.Code;
        question.PromptResourceKey = updatedQuestion.PromptResourceKey;
        question.IsRequired = updatedQuestion.IsRequired;
        question.Group = updatedQuestion.Group;
        question.QuestionVisibility = updatedQuestion.QuestionVisibility;
        question.ScoringWeight = updatedQuestion.ScoringWeight;
        question.ExplanationTemplateKey = updatedQuestion.ExplanationTemplateKey;
        dbContext.QuestionOptions.RemoveRange(question.Options);
        question.Options = updatedQuestion.Options.Select(option => new QuestionOption
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            OptionCode = option.OptionCode,
            LabelResourceKey = option.LabelResourceKey,
            SortOrder = option.SortOrder,
            ScoringValue = option.ScoringValue
        }).ToList();
        question.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.AuditEvents.Add(CreateAuditEvent(actorId, "Question.Updated", "QuestionDefinition", question.Id.ToString(), new { question.Code, question.UpdatedAt }));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static void ValidateQuestion(Question question)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question.Code);
        ArgumentException.ThrowIfNullOrWhiteSpace(question.PromptResourceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(question.Group);

        if (question.ScoringWeight < 0)
            throw new ArgumentOutOfRangeException(nameof(question), "Question scoring weight must not be negative.");

        if ((question.Type == QuestionType.MultipleChoice || question.Type == QuestionType.SingleChoice) && question.Options.Count == 0)
            throw new ArgumentException("Choice questions must define at least one option.", nameof(question));

        if (question.Options.Any(option => string.IsNullOrWhiteSpace(option.OptionCode) || string.IsNullOrWhiteSpace(option.LabelResourceKey)))
            throw new ArgumentException("Question options require stable IDs and label resource keys.", nameof(question));

        if (question.Options.Select(option => option.OptionCode).Distinct(StringComparer.Ordinal).Count() != question.Options.Count
            || question.Options.Select(option => option.SortOrder).Distinct().Count() != question.Options.Count)
            throw new ArgumentException("Question option IDs and sort orders must be unique.", nameof(question));
    }

    private async Task<Guid> RequireAdministratorAsync(ClaimsPrincipal actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        var authorizationResult = await authorizationService.AuthorizeAsync(actor, "AdministratorOnly");
        if (!authorizationResult.Succeeded)
            throw new UnauthorizedAccessException("Administrator authorization is required.");

        var actorId = actor.FindFirstValue(ClaimTypes.NameIdentifier);
        return !Guid.TryParse(actorId, out var parsedActorId)
            ? throw new UnauthorizedAccessException("An authenticated administrator identity is required.")
            : parsedActorId;
    }

    private static string NormalizeDomain(string domain)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        var normalized = domain.Trim().TrimEnd('.').ToLowerInvariant();
        if (normalized.Length > 253 || normalized.Contains('@') || Uri.CheckHostName(normalized) is not UriHostNameType.Dns)
            throw new ArgumentException("A valid DNS email domain is required.", nameof(domain));

        return normalized;
    }

    private static AuditEvent CreateAuditEvent(Guid actorId, string action, string entityType, string entityId, object details) => new()
    {
        Id = Guid.NewGuid(),
        ActorUserId = actorId,
        Action = action,
        EntityType = entityType,
        EntityId = entityId,
        OccurredAt = DateTimeOffset.UtcNow,
        DetailsJson = JsonSerializer.Serialize(details)
    };
}
