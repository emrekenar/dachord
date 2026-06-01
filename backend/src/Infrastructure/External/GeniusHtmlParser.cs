namespace Infrastructure.External;

using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Domain.Models.Track;

public static class GeniusHtmlParser
{
    private static readonly HashSet<string> KnownSectionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Intro", "Verse", "Pre-Chorus", "Chorus", "Bridge", "Interlude", "Solo", "Outro", "Hook", "Refrain"
    };


    public static List<Section>? Parse(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var containers = doc.DocumentNode.SelectNodes("//div[@data-lyrics-container='true']");
        if (containers == null || containers.Count == 0)
            return null;

        var sb = new StringBuilder();
        foreach (var container in containers)
            ExtractText(container, sb, isRoot: true);

        return BuildSections(sb.ToString());
    }

    private static void ExtractText(HtmlNode node, StringBuilder sb, bool isRoot = false)
    {
        if (node.Name is "script" or "style")
            return;

        // Nested divs inside the lyrics container hold metadata (contributor count, song title),
        // not lyric content. Actual lyrics are plain text nodes, <br>, and <h2> section headers.
        if (node.Name == "div" && !isRoot)
            return;

        if (node.NodeType == HtmlNodeType.Text)
        {
            sb.Append(HtmlEntity.DeEntitize(node.InnerText));
            return;
        }

        if (node.Name == "br")
        {
            sb.AppendLine();
            return;
        }

        foreach (var child in node.ChildNodes)
            ExtractText(child, sb);

        if (node.Name is "p" or "h1" or "h2" or "h3")
            sb.AppendLine();
    }

    private static List<Section> BuildSections(string text)
    {
        var sections = new List<Section>();
        Section? current = null;

        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();

            if (IsSectionHeader(line))
            {
                current = new Section { Type = NormalizeSectionType(line) };
                sections.Add(current);
                continue;
            }

            if (string.IsNullOrEmpty(line))
                continue;

            if (current == null)
            {
                current = new Section { Type = "" };
                sections.Add(current);
            }

            current.Lines.Add(new Line { Lyrics = line });
        }

        return sections.Where(s => s.Lines.Count > 0).ToList();
    }

    private static bool IsSectionHeader(string line) =>
        line.StartsWith('[') && line.EndsWith(']');

    private static string NormalizeSectionType(string label)
    {
        // Strip surrounding brackets
        var inner = label.Trim('[', ']').Trim();

        // Drop artist attribution: "Verse 1: Artist Name" → "Verse 1"
        var colonIdx = inner.IndexOf(':');
        if (colonIdx >= 0)
            inner = inner[..colonIdx].Trim();

        // Drop trailing number: "Verse 1" → "Verse"
        inner = Regex.Replace(inner, @"\s+\d+$", "").Trim();

        return KnownSectionTypes.Contains(inner) ? inner : "";
    }
}
