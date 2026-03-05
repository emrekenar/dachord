namespace Domain.Models;

public class Lyrics
{
    public string Content { get; set; } = string.Empty;
    public Dictionary<int, List<Chord>> Chords { get; set; } = new();
    public string? Language { get; set; }
}