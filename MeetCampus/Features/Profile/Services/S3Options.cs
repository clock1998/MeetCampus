namespace MeetCampus.Features.Profile.Services;

public sealed class S3Options
{
    public string Endpoint { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    public string AccessId { get; set; } = string.Empty;

    public string AccessSecret { get; set; } = string.Empty;
}
