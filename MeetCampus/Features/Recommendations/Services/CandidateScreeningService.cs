using System.Data;
using MeetCampus.Configuration;
using MeetCampus.Data;
using MeetCampus.Data.Matching;
using MeetCampus.Data.User;
using MeetCampus.Features.Account.Services;
using MeetCampus.Features.Matching.Models;
using MeetCampus.Features.Matching.Services;
using MeetCampus.Features.Recommendations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MeetCampus.Features.Recommendations.Services;

public sealed class CandidateScreeningService(
    ApplicationDbContext dbContext,
    AccountEligibilityService accountEligibilityService,
    MatchEvaluationService matchEvaluationService,
    IOptions<MatchingOptions> matchingOptions,
    ILogger<CandidateScreeningService> logger)
{
    public async Task<CandidateScreeningResult> ScreenAsync(ApplicationUser viewer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewer);
        var viewerUserId = viewer.Id;
        if (viewerUserId == Guid.Empty)
            throw new ArgumentException("A viewer ID is required.", nameof(viewerUserId));

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.RepeatableRead,
            cancellationToken);

        var viewerEligibility = await accountEligibilityService.GetEligibilityAsync(viewer, cancellationToken);
        if (!viewerEligibility.IsEligibleForDiscoverability)
        {
            await transaction.CommitAsync(cancellationToken);
            return new CandidateScreeningResult(false, []);
        }

        var excludedCandidateIds = dbContext.RecommendationItems.AsNoTracking()
            .Where(item => item.RecommendationBatch.ViewerUserId == viewerUserId);
        var suppressionCutoff = DateTimeOffset.UtcNow.AddDays(-matchingOptions.Value.PassedProfileSuppressionDays);
        var missingDecisionTimestampCount = await excludedCandidateIds
            .CountAsync(item => item.Decision == RecommendationDecision.Passed && item.DecidedAt == null, cancellationToken);
        if (missingDecisionTimestampCount > 0)
        {
            logger.LogWarning(
                "Found {Count} passed recommendation items without a decision timestamp; those candidates remain suppressed.",
                missingDecisionTimestampCount);
        }

        var candidateIds = await dbContext.Users.AsNoTracking()
            .Where(candidate => candidate.Id != viewerUserId
                && !dbContext.ContactRequests.Any(request =>
                    request.SenderUserId == viewerUserId && request.RecipientUserId == candidate.Id
                    || request.SenderUserId == candidate.Id && request.RecipientUserId == viewerUserId)
                && !dbContext.UserBlocks.Any(block =>
                    block.BlockerUserId == viewerUserId && block.BlockedUserId == candidate.Id
                    || block.BlockerUserId == candidate.Id && block.BlockedUserId == viewerUserId)
                && !excludedCandidateIds.Any(item => item.CandidateUserId == candidate.Id
                    && (item.Decision == RecommendationDecision.Unseen
                        || item.Decision == RecommendationDecision.Saved
                        || item.Decision == RecommendationDecision.Passed
                            && (item.DecidedAt == null || item.DecidedAt > suppressionCutoff))))
            .Select(candidate => candidate.Id)
            .ToListAsync(cancellationToken);

        var screenedCandidates = new List<ScreenedCandidate>();
        foreach (var candidateId in candidateIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var evaluation = await matchEvaluationService.EvaluateAsync(viewerUserId, candidateId, cancellationToken);
            if (evaluation.Status != MatchEvaluationStatus.Completed
                || evaluation.Score?.Score is not decimal score
                || score < matchingOptions.Value.MinimumCompatibilityScore
                || evaluation.Explanation is null)
                continue;

            screenedCandidates.Add(new ScreenedCandidate(candidateId, evaluation.Score, evaluation.Explanation));
        }

        var orderedCandidates = screenedCandidates
            .OrderByDescending(candidate => candidate.Score.Score)
            .ThenBy(candidate => candidate.CandidateUserId)
            .ToArray();
        await transaction.CommitAsync(cancellationToken);
        return new CandidateScreeningResult(true, orderedCandidates);
    }
}