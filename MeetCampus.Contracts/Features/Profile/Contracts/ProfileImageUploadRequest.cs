namespace MeetCampus.Contracts.Features.Profile.Contracts;

public sealed record ProfileImageUploadRequest(Stream Content, string FileName, string ContentType);
