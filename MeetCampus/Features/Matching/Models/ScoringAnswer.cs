namespace MeetCampus.Features.Matching.Models;

public sealed record ScoringAnswer(
    IReadOnlyList<Guid> OptionIds,
    int? ScaleValue);
