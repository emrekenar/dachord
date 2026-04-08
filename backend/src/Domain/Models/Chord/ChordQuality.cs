namespace Domain.Models.Chord;

public class ChordQuality
{
    public string Id { get; set; } = string.Empty; // e.g. "m"
    public List<int> Intervals { get; set; } = new();
}