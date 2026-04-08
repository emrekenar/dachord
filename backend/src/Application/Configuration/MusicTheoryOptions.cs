namespace Application.Configuration;

using Domain.Models.Chord;

public class MusicTheoryOptions
{
    public const string SectionName = "MusicTheory";

    public Dictionary<string, int> Notes { get; set; } = new();
    public List<ChordQuality> ChordQualities { get; set; } = new();
}