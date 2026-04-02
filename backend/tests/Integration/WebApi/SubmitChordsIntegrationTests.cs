using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.TestInfrastructure;

using Application.Requests;
using Domain.Models;

namespace IntegrationTests.WebApi;

public class SubmitChordsIntegrationTests : IClassFixture<IntegrationFixture>
{   
    private readonly IntegrationFixture _fixture;

    private static readonly string Endpoint = "/tracks";
    private static readonly HttpMethod Method = HttpMethod.Post;

    public SubmitChordsIntegrationTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SubmitChords_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = BuildRequest();

        // Act
        var response = await _fixture.SendRequestAsync(Method, Endpoint, request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitChords_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = BuildRequest();

        // Act
        var response = await _fixture.SendRequestAsync(Method, Endpoint, request, "InvalidToken");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitChords_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = BuildRequest();

        // Act
        var response = await _fixture.SendRequestAsAuthenticatedUserAsync(Method, Endpoint, request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    private SubmitChordsRequest BuildRequest(
        string? title = null, 
        string? artist = null, 
        string? lyricsContent = null, 
        Dictionary<int, List<Chord>>? chords = null, 
        string? language = null)
    {
        var request = new SubmitChordsRequest
        {
            Title = title ?? "Test Track",
            Artist = artist ?? "Test Artist",
            Lyrics = new Lyrics
            {
                Content = lyricsContent ?? "Test lyrics content\nWith multiple lines.",
                Chords = chords ?? new Dictionary<int, List<Chord>>(),
                Language = language ?? "en",
            }
        };
        return request;
    }
}
