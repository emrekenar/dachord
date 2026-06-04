using FluentAssertions;
using NSubstitute;
using Xunit;

using Domain.Interfaces;
using Domain.Models.User;
using Application.Interfaces;
using Application.Requests;
using Application.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Domain.Models.Track;

namespace UnitTests.Application.Services;

public class SubmitChordsServiceTests
{
    private readonly ISubmitChordsService _sut;
    private readonly IUserRepository _userRepo;

    public SubmitChordsServiceTests()
    {
        var existingTrack = new Track
        {
            Id = "track001",
            Title = "Test track",
            ArtistId = "artist001",
            ArtistName = "Test Artist",
            AlbumId = "album001",
            AlbumName = "Test Album"
        };

        var trackRepo = Substitute.For<ITrackRepository>();
        trackRepo.GetTrackAsync("track001").Returns(existingTrack);

        _userRepo = Substitute.For<IUserRepository>();
        _userRepo.GetByIdAsync("user001").Returns(new User
        {
            Id = "user001",
            Email = "user@test.com",
            PasswordHash = "hash",
            DisplayName = "User 1"
        });

        var searchTracksService = Substitute.For<ISearchTracksService>();
        var logger = NullLogger<SubmitChordsService>.Instance;
        _sut = new SubmitChordsService(trackRepo, _userRepo, searchTracksService, logger);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSaveTrackAndReturnResponse()
    {
        // Arrange
        var request = new SubmitChordsRequest
        {
            TrackId = "track001",
            ContributorId = "user001",
            ContributorEmail = "user@test.com",
            Content =
            [
                new Section
                {
                    Type = "Verse 1",
                    Lines =
                    [
                        new Line
                        {
                            Lyrics = "This is Line 1",
                            Chords =
                            {
                                ["0"] = "Em",
                                ["10"] = "Dm",
                            }
                        }
                    ]
                }
            ]
        };

        // Act
        var response = await _sut.ExecuteAsync(request);

        // Assert
        response.IsSuccess.Should().BeTrue();
        var result = response.Value;
        result.Should().NotBeNull();
        result.TrackId.Should().Be("track001");
        result.ContributorId.Should().Be("user001");
        result.ContributorName.Should().Be("User 1");
        result.ContributorEmail.Should().Be("user@test.com");
        result.Content.Should().NotBeNullOrEmpty();
        result.Content[0].Type.Should().Be("Verse 1");
        result.Content[0].Lines.Should().NotBeNullOrEmpty();
        result.Content[0].Lines[0].Lyrics.Should().Be("This is Line 1");
        result.Content[0].Lines[0].Chords.Should().NotBeNullOrEmpty();
        result.Content[0].Lines[0].Chords["0"].Should().Be("Em");
        result.Content[0].Lines[0].Chords["10"].Should().Be("Dm");
    }

    [Fact]
    public async Task ExecuteAsync_EmptyContent_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new SubmitChordsRequest
        {
            TrackId = "track001",
            ContributorId = "user001",
            Content = [],
        };

        // Act
        var response = await _sut.ExecuteAsync(request);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Error!.Code.Should().Be(Domain.Errors.ErrorCode.InvalidRequest);
    }
}
