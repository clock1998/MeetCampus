namespace MeetCampus.Features.Matching.Models;

public sealed record ScoringCategory(Guid Id, string Code, decimal MatchingWeight, string NameEn, string NameFr, int SortOrder);
