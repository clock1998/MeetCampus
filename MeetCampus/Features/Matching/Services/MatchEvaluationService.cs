using MeetCampus.Features.Matching.Models;

namespace MeetCampus.Features.Matching.Services;

public sealed class MatchEvaluationService(
    MatchEligibilityService eligibilityService,
    ScoringInputService inputService)
{
    /// <summary>Evaluates a pair for server-side recommendation selection without retaining their answers.</summary>
    public async Task<MatchEvaluationResult> EvaluateAsync(Guid firstUserId, Guid secondUserId, CancellationToken cancellationToken = default)
    {
        var status = await eligibilityService.GetStatusAsync(firstUserId, secondUserId, cancellationToken);
        if (status != MatchEvaluationStatus.Completed)
            return new MatchEvaluationResult(status, null, null);

        var input = await inputService.LoadAsync(firstUserId, secondUserId, cancellationToken);
        var firstMissing = input.Questions.Any(question => question.FirstAnswer is null);
        var secondMissing = input.Questions.Any(question => question.SecondAnswer is null);
        if (firstMissing || secondMissing)
        {
            status = await eligibilityService.GetStatusAsync(firstUserId, secondUserId, cancellationToken);
            if (status == MatchEvaluationStatus.Completed)
            {
                status = (firstMissing, secondMissing) switch
                {
                    (true, true) => MatchEvaluationStatus.BothMembersAnswersIncomplete,
                    (true, false) => MatchEvaluationStatus.FirstMemberAnswersIncomplete,
                    _ => MatchEvaluationStatus.SecondMemberAnswersIncomplete
                };
            }

            return new MatchEvaluationResult(status, null, null);
        }

        var score = CompatibilityScoringService.Calculate(input);
        if (score.Score is null)
            return new MatchEvaluationResult(MatchEvaluationStatus.ScoreUnavailable, score, null);

        var explanation = MatchExplanationService.Explain(score, input.Categories);
        return new MatchEvaluationResult(MatchEvaluationStatus.Completed, score, explanation);
    }
}
