using MeetCampus.Contracts.Features.Administration;

namespace MeetCampus.Features.Matching.Models;

public sealed record ScoringQuestion(
    Guid Id,
    QuestionType Type,
    string Category,
    decimal ScoringWeight,
    int? ScaleMin,
    int? ScaleMax,
    ScoringAnswer? FirstAnswer,
    ScoringAnswer? SecondAnswer);
