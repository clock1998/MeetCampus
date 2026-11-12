namespace MeetCampus.Features.Matching.Models;

public sealed record CategoryScore(string Category, decimal? Score, int QuestionCount);
