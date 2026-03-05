namespace Domain.Models;

public class Track
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public Lyrics? Lyrics { get; set; }
}