namespace Application.Services;

using Application.Configuration;
using Microsoft.Extensions.Options;

public class ChordService
{
    private readonly MusicTheoryOptions _options;

    public ChordService(IOptions<MusicTheoryOptions> options)
    {
        _options = options.Value;
    }

    public List<string> GetAvailableChords()
    {
        var allChords = new List<string>();
        
        // Combine every note with every quality for frontend dropdowns
        foreach (var note in _options.Notes.Keys)
        {
            foreach (var quality in _options.ChordQualities)
            {
                // Result: "C", "Cm", "Cmaj7", etc.
                allChords.Add($"{note}{quality.Id}");
            }
        }
        return allChords;
    }
}