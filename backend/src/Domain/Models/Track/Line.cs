namespace Domain.Models.Track;

public class Line
{
    public string Lyrics { get; set; } = string.Empty;
    // Keys are character positions as strings, e.g. "0": "C", "10": "G"
    public Dictionary<string, string> Chords { get; set; } = new();
}