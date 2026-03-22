using FluentAssertions;
using NSubstitute;
using Xunit;

using Domain.Interfaces;
using Domain.Models;
using Application.Interfaces;
using Application.Requests;
using Application.Services;

namespace UnitTests.Application.Services;

public class SubmitChordsServiceTests
{
    private readonly ISubmitChordsService _sut;

    public SubmitChordsServiceTests()
    {
        var trackRepo = Substitute.For<ITrackRepository>();
        _sut = new SubmitChordsService(trackRepo);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSaveTrackAndReturnResponse()
    {
        // Arrange
        var request = new SubmitChordsRequest
        {
            Title = "Test Song",
            Artist = "Test Artist",
            Lyrics = new Lyrics
            {
                Content = "Test lyrics content",
                Chords = new Dictionary<int, List<Chord>>
                {
                    { 0, new List<Chord> { new Chord { Signature = "C", Notes = new List<Note> { new Note { Signature = "C" }, new Note { Signature = "E" }, new Note { Signature = "G" } } } } }, 
                    { 5, new List<Chord> { new Chord { Signature = "G", Notes = new List<Note> { new Note { Signature = "G" }, new Note { Signature = "B" }, new Note { Signature = "D" } } } } }
                },
                Language = "en",
            }
        };

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Title.Should().Be(request.Title);
        result.Value.Artist.Should().Be(request.Artist);
        result.Value.Lyrics.Should().NotBeNull();
        result.Value.Lyrics.Content.Should().Be(request.Lyrics.Content);
        result.Value.Lyrics.Chords.Should().HaveCount(2);
        result.Value.Lyrics.Chords[0].Should().HaveCount(1);
        result.Value.Lyrics.Chords[0][0].Signature.Should().Be("C");
        result.Value.Lyrics.Chords[0][0].Notes.Should().HaveCount(3);
        result.Value.Lyrics.Chords[5].Should().HaveCount(1);
        result.Value.Lyrics.Chords[5][0].Signature.Should().Be("G");
        result.Value.Lyrics.Chords[5][0].Notes.Should().HaveCount(3);
        result.Value.Lyrics.Language.Should().Be("en");
    }
}