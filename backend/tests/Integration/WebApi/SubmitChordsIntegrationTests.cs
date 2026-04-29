using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.TestInfrastructure;

using Application.Requests;
using Domain.Models.Track;

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
        string? trackId = IntegrationFixture.ExistingTrackId,
        string? contributorId = IntegrationFixture.ExistingTestUserId,
        string? contributorName = IntegrationFixture.ExistingTestUserDisplayName,
        string? contributorEmail = IntegrationFixture.ExistingTestUserEmail,
        List<Section>? content = null)
    {
        var request = new SubmitChordsRequest
        {
            TrackId = trackId!,
            ContributorId = contributorId!,
            ContributorName = contributorName,
            ContributorEmail = contributorEmail,
            Content = content ?? [
                new Section
                {
                    Type = "Verse",
                    Lines = [
                        new Line
                        {
                            Lyrics = "This is Line 1",
                            Chords = { ["0"] = "Em", ["8"] = "D" },
                        },
                        new Line
                        {
                            Lyrics = "This is Line 2",
                            Chords = { ["5"] = "C", ["7"] = "Em" },
                        },
                    ]
                }
            ]
        };
        return request;
    }
}
