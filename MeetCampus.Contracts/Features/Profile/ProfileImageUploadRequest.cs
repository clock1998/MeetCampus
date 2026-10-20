namespace MeetCampus.Contracts.Features.Profile;

public sealed record ProfileImageUploadRequest(Stream Content, string FileName, string ContentType);
