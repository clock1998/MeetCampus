namespace MeetCampus.Contracts.Features.Administration;

public sealed class QuestionAdminException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
