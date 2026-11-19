using MeetCampus.Features.Matching.Models;

namespace MeetCampus.Features.Recommendations.Models;

public sealed record ScreenedCandidate(
    Guid CandidateUserId,
    CompatibilityScoreResult Score,
    MatchExplanationResult Explanation);