namespace Domain.Models;

public class Track
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public Lyrics? Lyrics { get; set; }
}