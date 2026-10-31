using MeetCampus.Contracts.Features.Administration;

namespace MeetCampus.Features.Administration.Services;

public static class QuestionnaireAdminCatalog
{
    public static QuestionCatalogResponse Value { get; } = new(
    [
        new("interests", "Administration.Questionnaire.Category.Interests"),
        new("values", "Administration.Questionnaire.Category.Values"),
        new("social", "Administration.Questionnaire.Category.Social"),
        new("lifestyle", "Administration.Questionnaire.Category.Lifestyle"),
        new("connection-intent", "Administration.Questionnaire.Category.ConnectionIntent")
    ]);
}
