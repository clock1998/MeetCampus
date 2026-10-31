using System.Security.Claims;
using System.Text.Json;
using MeetCampus.Data;
using MeetCampus.Data.Audit;
using MeetCampus.Data.Matching;
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
