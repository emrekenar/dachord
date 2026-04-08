namespace Domain.Models.Track;

public class Section
{
    public string Type { get; set; } = string.Empty; // Verse, Bridge, Intro
    public List<Line> Lines { get; set; } = new();
}