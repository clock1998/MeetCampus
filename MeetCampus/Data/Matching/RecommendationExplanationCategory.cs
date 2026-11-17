using MeetCampus.Data.Questionnaire;

namespace MeetCampus.Data.Matching;

public sealed class RecommendationExplanationCategory
{
    public Guid RecommendationItemId { get; set; }
    public RecommendationItem RecommendationItem { get; set; } = default!;
    public Guid QuestionCategoryId { get; set; }
    public QuestionCategory QuestionCategory { get; set; } = default!;
    public int SortOrder { get; set; }
}
