namespace MeetCampus.Data;

public sealed class Gender
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string DisplayKey { get; set; } = string.Empty;
}
