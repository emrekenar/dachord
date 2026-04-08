namespace Domain.Models.Track;

public class Line
{
    public string Lyrics { get; set; } = string.Empty;
    // Index 0: "C", Index 10: "G" (Chords placed at specific character positions)
    public Dictionary<int, string> Chords { get; set; } = new();
}