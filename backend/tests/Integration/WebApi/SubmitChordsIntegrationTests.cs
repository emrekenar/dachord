using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.TestInfrastructure;

using Application.Requests;
using Domain.Models;

namespace IntegrationTests.WebApi;

public class SubmitChordsIntegrationTests : IClassFixture<IntegrationFixture>
{   
    private readonly IntegrationFixture _fixture;

    public SubmitChordsIntegrationTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SubmitChords_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new SubmitChordsRequest
        {
            Title = "Test Track",
            Artist = "Test Artist",
            Lyrics = new Lyrics
            {
                Content = "Test lyrics content",
                Chords = new Dictionary<int, List<Chord>>(),
                Language = "en",
            }
        };

        // Act
        var response = await _fixture.Client.PostAsJsonAsync("/tracks", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
