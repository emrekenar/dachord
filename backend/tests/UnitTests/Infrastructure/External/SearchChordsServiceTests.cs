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

namespace UnitTests.Infrastructure.External;

public class SearchChordsServiceTests
{
    private readonly ISearchChordsService _sut;

    public SearchChordsServiceTests()
    {
        var existingTrackVersion = new TrackVersion
        {
            TrackId = "track1",
            ContributorId = "contributor1",
            ContributorEmail = "user@test.com",
            Content = [new Section()],
        };
        var existingTrack = new Track
        {
            Id = "track1",
            Title = "Existing track",
            ArtistId = "artist1",
            ArtistName = "Artist 1",
            AlbumId = "album1",
            AlbumName = "Album 1"
        };
        var newTrack = new Track
        {
            Id = "track2",
            Title = "New track",
            ArtistId = "artist2",
            ArtistName = "New Artist",
            AlbumId = "album2",
            AlbumName = "New Album"
        };

        var searchTracksService = Substitute.For<ISearchTracksService>();
        searchTracksService.QueryAsync(Arg.Any<TrackSearchRequest>()).Returns(
            new SearchTracksResponse([
                TrackDtoMapper.MapToResponse(existingTrack),
                TrackDtoMapper.MapToResponse(newTrack),
            ])
        );

        var trackRepo = Substitute.For<ITrackRepository>();
        trackRepo.GetTrackVersionsAsync("track1").Returns(
            Task.FromResult<IEnumerable<TrackVersion>>([existingTrackVersion])
        );
        trackRepo.GetTrackVersionsAsync(Arg.Is<string>(s => s != "track1")).Returns(
            Task.FromResult<IEnumerable<TrackVersion>>([])
        );

        var logger = NullLogger<SearchChordsService>.Instance;
        _sut = new SearchChordsService(searchTracksService, trackRepo, logger);
    }

    [Fact]
    public async Task ExecuteAsync_ValidQuery_ReturnsTwoTracksWithCorrectVersions()
    {
        // Arrange
        var request = new TrackSearchRequest { Query = "tra" };

        // Act
        var response = await _sut.ExecuteAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Results.Should().HaveCount(2);

        var firstPair = response.Results[0];
        firstPair.Track.TrackId.Should().Be("track1");
        firstPair.Versions.Should().HaveCount(1);
        firstPair.Versions[0].ContributorId.Should().Be("contributor1");

        var secondPair = response.Results[1];
        secondPair.Track.TrackId.Should().Be("track2");
        secondPair.Versions.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShortQuery_ReturnsNull()
    {
        // Arrange
        var request = new TrackSearchRequest { Query = "ab" };

        // Act
        var response = await _sut.ExecuteAsync(request);

        // Assert
        response.Should().BeNull();
    }
}
