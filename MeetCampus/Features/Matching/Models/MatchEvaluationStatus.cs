namespace MeetCampus.Features.Matching.Models;

public enum MatchEvaluationStatus
{
    Ineligible,
    FirstMemberAnswersIncomplete,
    SecondMemberAnswersIncomplete,
    BothMembersAnswersIncomplete,
    ScoreUnavailable,
    Completed
}
