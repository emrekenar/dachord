namespace Infrastructure.External;

using System.Text.RegularExpressions;
using Domain.Models.Track;

public static class LrclibLyricsParser
{
    private static readonly Regex LrcTimestamp = new(@"^\[(\d{2}):(\d{2}\.\d{2})\]\s?(.*)$", RegexOptions.Compiled);

    public static List<Section> ParseSynced(string syncedLyrics)
    {
        var sections = new List<Section>();
        Section? current = null;

        foreach (var raw in syncedLyrics.Split('\n'))
        {
            var m = LrcTimestamp.Match(raw.Trim());
            if (!m.Success) continue;

            var minutes = int.Parse(m.Groups[1].Value);
            var seconds = double.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
            var timeMs = (int)((minutes * 60 + seconds) * 1000);
            var lyrics = m.Groups[3].Value;

            if (string.IsNullOrEmpty(lyrics))
            {
                current = null;
                continue;
            }

            if (current == null)
            {
                current = new Section { Type = "" };
                sections.Add(current);
            }

            current.Lines.Add(new Line { Lyrics = lyrics, TimeMs = timeMs });
        }

        return sections.Where(s => s.Lines.Count > 0).ToList();
    }

    public static List<Section> Parse(string plainLyrics)
    {
        var sections = new List<Section>();
        Section? current = null;

        foreach (var raw in plainLyrics.Split('\n'))
        {
            var line = raw.Trim();

            if (string.IsNullOrEmpty(line))
            {
                current = null;
                continue;
            }

            if (current == null)
            {
                current = new Section { Type = "" };
                sections.Add(current);
            }

            current.Lines.Add(new Line { Lyrics = line });
        }

        return sections.Where(s => s.Lines.Count > 0).ToList();
    }
}
