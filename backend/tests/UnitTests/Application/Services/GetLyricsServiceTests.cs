namespace UnitTests.Application.Services;

using FluentAssertions;
using NSubstitute;
using Xunit;
using global::Application.Interfaces;
using global::Application.Services;
using Domain.Errors;
using Domain.Models.Track;

public class GetLyricsServiceTests
{
    private readonly ISearchTracksService _searchTracksService = Substitute.For<ISearchTracksService>();
    private readonly ILyricsService _lyricsService = Substitute.For<ILyricsService>();
    private readonly GetLyricsService _sut;

    public GetLyricsServiceTests()
    {
        _sut = new GetLyricsService(_searchTracksService, _lyricsService);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTrackFoundAndLyricsFound_ReturnsOkWithSections()
    {
        var track = new Track { Id = "t1", Title = "Song", ArtistName = "Artist", ArtistId = "a1", AlbumId = "al1", AlbumName = "Album" };
        var sections = new List<Section>
        {
            new() { Type = "Verse", Lines = [new Line { Lyrics = "Line one" }] },
            new() { Type = "Chorus", Lines = [new Line { Lyrics = "Line two" }] },
        };
        _searchTracksService.GetTrackAsync("t1").Returns(track);
        _lyricsService.GetLyricsAsync("Artist", "Song").Returns(sections);

        var result = await _sut.ExecuteAsync("t1");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Sections.Should().HaveCount(2);
        result.Value.Sections[0].Type.Should().Be("Verse");
    }

    [Fact]
    public async Task ExecuteAsync_WhenTrackNotFound_ReturnsTrackNotFoundError()
    {
        _searchTracksService.GetTrackAsync("unknown").Returns((Track?)null);

        var result = await _sut.ExecuteAsync("unknown");

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.TrackNotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLyricsServiceReturnsNull_ReturnsLyricsNotFoundError()
    {
        var track = new Track { Id = "t1", Title = "Song", ArtistName = "Artist", ArtistId = "a1", AlbumId = "al1", AlbumName = "Album" };
        _searchTracksService.GetTrackAsync("t1").Returns(track);
        _lyricsService.GetLyricsAsync("Artist", "Song").Returns((List<Section>?)null);

        var result = await _sut.ExecuteAsync("t1");

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.LyricsNotFound);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectArtistAndTitleToLyricsService()
    {
        var track = new Track { Id = "t2", Title = "My Song", ArtistName = "My Artist", ArtistId = "a2", AlbumId = "al2", AlbumName = "Album" };
        _searchTracksService.GetTrackAsync("t2").Returns(track);
        _lyricsService.GetLyricsAsync(Arg.Any<string>(), Arg.Any<string>()).Returns((List<Section>?)null);

        await _sut.ExecuteAsync("t2");

        await _lyricsService.Received(1).GetLyricsAsync("My Artist", "My Song");
    }
}
