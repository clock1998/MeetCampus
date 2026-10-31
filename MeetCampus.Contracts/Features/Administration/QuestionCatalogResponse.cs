namespace MeetCampus.Contracts.Features.Administration;

public sealed record QuestionCatalogResponse(
    IReadOnlyList<QuestionCategoryOption> Categories);
