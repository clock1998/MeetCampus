namespace MeetCampus.Features.Recommendations.Models;

public sealed record CandidateScreeningResult(
    bool ViewerIsEligible,
    IReadOnlyList<ScreenedCandidate> Candidates);