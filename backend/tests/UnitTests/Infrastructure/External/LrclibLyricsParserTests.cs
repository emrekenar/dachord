namespace UnitTests.Infrastructure.External;

using FluentAssertions;
using Xunit;
using global::Infrastructure.External;

public class LrclibLyricsParserTests
{
    // --- ParseSynced ---

    [Fact]
    public void ParseSynced_TimestampsAreExtracted()
    {
        var sections = LrclibLyricsParser.ParseSynced("[00:19.16] When you were here before\n[00:24.09] Couldn't look you in the eye");

        sections.Should().HaveCount(1);
        sections[0].Lines[0].Lyrics.Should().Be("When you were here before");
        sections[0].Lines[0].TimeMs.Should().Be(19160);
        sections[0].Lines[1].Lyrics.Should().Be("Couldn't look you in the eye");
        sections[0].Lines[1].TimeMs.Should().Be(24090);
    }

    [Fact]
    public void ParseSynced_EmptyTimestampLine_StartNewSection()
    {
        var input = "[00:10.00] Line one\n[00:20.00] \n[00:30.00] Line two";

        var sections = LrclibLyricsParser.ParseSynced(input);

        sections.Should().HaveCount(2);
        sections[0].Lines.Should().HaveCount(1);
        sections[0].Lines[0].Lyrics.Should().Be("Line one");
        sections[1].Lines.Should().HaveCount(1);
        sections[1].Lines[0].Lyrics.Should().Be("Line two");
    }

    [Fact]
    public void ParseSynced_AllSectionsHaveNoType()
    {
        var sections = LrclibLyricsParser.ParseSynced("[01:00.00] A line");

        sections[0].Type.Should().Be("");
    }

    [Fact]
    public void ParseSynced_MinutesConvertedCorrectly()
    {
        var sections = LrclibLyricsParser.ParseSynced("[02:03.45] Line");

        sections[0].Lines[0].TimeMs.Should().Be(2 * 60 * 1000 + 3450);
    }

    // --- Parse (plain fallback) ---

    [Fact]
    public void Parse_BlankLineSeparatedGroups_CreatesSeparateSections()
    {
        var sections = LrclibLyricsParser.Parse("Line one\nLine two\n\nLine three\nLine four");

        sections.Should().HaveCount(2);
        sections[0].Lines.Should().HaveCount(2);
        sections[1].Lines.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_PlainLines_HaveNoTimestamp()
    {
        var sections = LrclibLyricsParser.Parse("A line");

        sections[0].Lines[0].TimeMs.Should().BeNull();
    }

    [Fact]
    public void Parse_LeadingAndTrailingWhitespace_IsTrimmed()
    {
        var sections = LrclibLyricsParser.Parse("  Line one  \n  Line two  ");

        sections[0].Lines[0].Lyrics.Should().Be("Line one");
        sections[0].Lines[1].Lyrics.Should().Be("Line two");
    }

    [Fact]
    public void Parse_EmptySections_AreExcluded()
    {
        var sections = LrclibLyricsParser.Parse("\n\nLine one");

        sections.Should().HaveCount(1);
        sections[0].Lines[0].Lyrics.Should().Be("Line one");
    }
}
