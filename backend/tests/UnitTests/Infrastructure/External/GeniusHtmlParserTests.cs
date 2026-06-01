namespace UnitTests.Infrastructure.External;

using FluentAssertions;
using Xunit;
using global::Infrastructure.External;

public class GeniusHtmlParserTests
{
    [Fact]
    public void Parse_SingleSectionWithHeader_ReturnsSectionWithCorrectTypeAndLines()
    {
        var html = LyricsPage("[Verse 1]\nLine one<br>Line two");

        var sections = GeniusHtmlParser.Parse(html);

        sections.Should().HaveCount(1);
        sections![0].Type.Should().Be("Verse");
        sections[0].Lines.Should().HaveCount(2);
        sections[0].Lines[0].Lyrics.Should().Be("Line one");
        sections[0].Lines[1].Lyrics.Should().Be("Line two");
    }

    [Theory]
    [InlineData("[Verse 1]", "Verse")]
    [InlineData("[Chorus 2]", "Chorus")]
    [InlineData("[Pre-Chorus]", "Pre-Chorus")]
    [InlineData("[Bridge]", "Bridge")]
    [InlineData("[Outro 3]", "Outro")]
    [InlineData("[Interlude]", "Interlude")]
    [InlineData("[Verse 1: Artist Name]", "Verse")]
    [InlineData("[Unknown Part]", "")]
    [InlineData("[Some Random Label]", "")]
    public void Parse_SectionLabelNormalization_ReturnsExpectedType(string header, string expectedType)
    {
        var html = LyricsPage($"{header}\nA line");

        var sections = GeniusHtmlParser.Parse(html);

        sections.Should().NotBeNullOrEmpty();
        sections![0].Type.Should().Be(expectedType);
    }

    [Fact]
    public void Parse_LinesBeforeFirstHeader_BecomeUnlabeledSection()
    {
        var html = LyricsPage("Intro line<br>[Chorus]\nChorus line");

        var sections = GeniusHtmlParser.Parse(html);

        sections.Should().HaveCount(2);
        sections![0].Type.Should().Be("");
        sections[0].Lines[0].Lyrics.Should().Be("Intro line");
        sections[1].Type.Should().Be("Chorus");
        sections[1].Lines[0].Lyrics.Should().Be("Chorus line");
    }

    [Fact]
    public void Parse_HtmlLineBreaks_SplitLinesCorrectly()
    {
        var html = LyricsPage("[Verse]\nLine one<br>Line two<br/>Line three");

        var sections = GeniusHtmlParser.Parse(html);

        sections.Should().HaveCount(1);
        sections![0].Lines.Should().HaveCount(3);
    }

    [Fact]
    public void Parse_EmptyLines_AreIgnored()
    {
        var html = LyricsPage("[Verse]\n\nLine one\n\nLine two\n");

        var sections = GeniusHtmlParser.Parse(html);

        sections.Should().HaveCount(1);
        sections![0].Lines.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_MultipleSections_ReturnsAllWithCorrectStructure()
    {
        var html = LyricsPage(
            "[Verse 1]\nV1 line one<br>V1 line two<br>V1 line three\n" +
            "[Chorus]\nC line one<br>C line two\n" +
            "[Bridge]\nB line one");

        var sections = GeniusHtmlParser.Parse(html);

        sections.Should().HaveCount(3);
        sections![0].Type.Should().Be("Verse");
        sections[0].Lines.Should().HaveCount(3);
        sections[1].Type.Should().Be("Chorus");
        sections[1].Lines.Should().HaveCount(2);
        sections[2].Type.Should().Be("Bridge");
        sections[2].Lines.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_SectionsWithNoLines_AreExcluded()
    {
        var html = LyricsPage("[Verse]\n[Chorus]\nChorus line");

        var sections = GeniusHtmlParser.Parse(html);

        sections.Should().HaveCount(1);
        sections![0].Type.Should().Be("Chorus");
    }

    [Fact]
    public void Parse_NestedDivsInsideContainer_AreSkipped()
    {
        // Genius wraps contributor count + song title in a nested <div> inside the lyrics container
        const string html = """
            <html><body>
            <div data-lyrics-container="true">
              <div class="some-metadata-class">29 ContributorsSong Title Lyrics</div>
              [Verse]
              Real lyric line<br>
            </div>
            </body></html>
            """;

        var sections = GeniusHtmlParser.Parse(html);

        sections.Should().HaveCount(1);
        sections![0].Lines.Should().HaveCount(1);
        sections[0].Lines[0].Lyrics.Should().Be("Real lyric line");
    }

    [Fact]
    public void Parse_NoLyricsContainerDiv_ReturnsNull()
    {
        const string html = "<html><body><div>No lyrics here</div></body></html>";

        var sections = GeniusHtmlParser.Parse(html);

        sections.Should().BeNull();
    }

    [Fact]
    public void Parse_AllChordsEmptyOnReturn()
    {
        var html = LyricsPage("[Verse]\nSome lyric line");

        var sections = GeniusHtmlParser.Parse(html);

        sections![0].Lines.Should().AllSatisfy(l => l.Chords.Should().BeEmpty());
    }

    private static string LyricsPage(string content) =>
        $"""<html><body><div data-lyrics-container="true">{content}</div></body></html>""";
}
