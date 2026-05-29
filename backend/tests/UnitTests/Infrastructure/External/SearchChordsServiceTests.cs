using FluentAssertions;
using NSubstitute;
using Xunit;

using Domain.Interfaces;
using Application.Interfaces;
using Application.Requests;
using Application.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Domain.Models.Track;
using Application.Mappers;
using Application.Responses;

namespace UnitTests.Infrastructure.External;

public class SearchChordsServiceTests
{
    private readonly ISearchTracksService _searchTracksService;
    private readonly ITrackRepository _trackRepo;
    private readonly ISearchChordsService _sut;

    private static readonly TrackResponse Track1 = new() { TrackId = "track1", Title = "Existing Track", ArtistId = "artist1", ArtistName = "Artist 1", AlbumId = "album1", AlbumName = "Album 1" };
    private static readonly TrackResponse Track2 = new() { TrackId = "track2", Title = "New Track", ArtistId = "artist2", ArtistName = "Artist 2", AlbumId = "album2", AlbumName = "Album 2" };

    private static readonly TrackVersion Track1Version = new()
    {
        TrackId = "track1",
        ContributorId = "contributor1",
        ContributorEmail = "user@test.com",
        Content = [new Section()],
    };

    public SearchChordsServiceTests()
    {
        _searchTracksService = Substitute.For<ISearchTracksService>();
        _trackRepo = Substitute.For<ITrackRepository>();

        _searchTracksService.QueryAsync(Arg.Any<TrackSearchRequest>())
            .Returns(new SearchTracksResponse([Track1, Track2]));

        _searchTracksService.GetTracksFromArtist(Arg.Any<string>())
            .Returns(new SearchTracksResponse([Track1]));

        _searchTracksService.GetTracksInAlbum(Arg.Any<string>())
            .Returns(new SearchTracksResponse([Track1, Track2]));

        _trackRepo.GetTrackVersionsAsync("track1")
            .Returns(Task.FromResult<IEnumerable<TrackVersion>>([Track1Version]));
        _trackRepo.GetTrackVersionsAsync(Arg.Is<string>(s => s != "track1"))
            .Returns(Task.FromResult<IEnumerable<TrackVersion>>([]));

        _sut = new SearchChordsService(_searchTracksService, _trackRepo, NullLogger<SearchChordsService>.Instance);
    }

    // ── Text search ───────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ValidQuery_ReturnsTwoTracksWithCorrectVersions()
    {
        var response = await _sut.ExecuteAsync(new TrackSearchRequest { Query = "tra" });

        response.Should().NotBeNull();
        response!.Results.Should().HaveCount(2);
        response.Results[0].Track.TrackId.Should().Be("track1");
        response.Results[0].Versions.Should().HaveCount(1);
        response.Results[0].Versions[0].ContributorId.Should().Be("contributor1");
        response.Results[1].Track.TrackId.Should().Be("track2");
        response.Results[1].Versions.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ValidQuery_CallsQueryAsync()
    {
        await _sut.ExecuteAsync(new TrackSearchRequest { Query = "tra" });

        await _searchTracksService.Received(1).QueryAsync(Arg.Any<TrackSearchRequest>());
        await _searchTracksService.DidNotReceive().GetTracksFromArtist(Arg.Any<string>());
        await _searchTracksService.DidNotReceive().GetTracksInAlbum(Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_ShortQuery_ReturnsNull()
    {
        var response = await _sut.ExecuteAsync(new TrackSearchRequest { Query = "ab" });

        response.Should().BeNull();
    }

    // ── Artist search ─────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WithArtistId_CallsGetTracksFromArtist()
    {
        await _sut.ExecuteAsync(new TrackSearchRequest { ArtistId = "artist1" });

        await _searchTracksService.Received(1).GetTracksFromArtist("artist1");
        await _searchTracksService.DidNotReceive().QueryAsync(Arg.Any<TrackSearchRequest>());
        await _searchTracksService.DidNotReceive().GetTracksInAlbum(Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WithArtistId_ReturnsTracksWithVersions()
    {
        var response = await _sut.ExecuteAsync(new TrackSearchRequest { ArtistId = "artist1" });

        response.Should().NotBeNull();
        response!.Results.Should().HaveCount(1);
        response.Results[0].Track.TrackId.Should().Be("track1");
        response.Results[0].Versions.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithArtistId_EmptyQueryDoesNotBlock()
    {
        // ArtistId bypasses the Query.Length <= 2 guard
        var response = await _sut.ExecuteAsync(new TrackSearchRequest { ArtistId = "artist1", Query = "" });

        response.Should().NotBeNull();
    }

    // ── Album search ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WithAlbumId_CallsGetTracksInAlbum()
    {
        await _sut.ExecuteAsync(new TrackSearchRequest { AlbumId = "album1" });

        await _searchTracksService.Received(1).GetTracksInAlbum("album1");
        await _searchTracksService.DidNotReceive().QueryAsync(Arg.Any<TrackSearchRequest>());
        await _searchTracksService.DidNotReceive().GetTracksFromArtist(Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAlbumId_ReturnsTracksWithVersions()
    {
        var response = await _sut.ExecuteAsync(new TrackSearchRequest { AlbumId = "album1" });

        response.Should().NotBeNull();
        response!.Results.Should().HaveCount(2);
        response.Results[0].Versions.Should().HaveCount(1);
        response.Results[1].Versions.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithAlbumId_EmptyQueryDoesNotBlock()
    {
        var response = await _sut.ExecuteAsync(new TrackSearchRequest { AlbumId = "album1", Query = "" });

        response.Should().NotBeNull();
    }
}
