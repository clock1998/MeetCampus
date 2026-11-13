namespace MeetCampus.Features.Matching.Models;

public sealed record ScoringInput(
    IReadOnlyList<ScoringCategory> Categories,
    IReadOnlyList<ScoringQuestion> Questions,
    IReadOnlyList<Guid> FirstIntentIds,
    IReadOnlyList<Guid> SecondIntentIds);
