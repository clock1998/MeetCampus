namespace MeetCampus.Features.Matching.Models;

public sealed record ScoringCategory(string Code, decimal MatchingWeight, string NameEn, string NameFr, int SortOrder);
