using FluentAssertions;
using E2ETests.TestInfrastructure;

using Application.Requests;
using Domain.Models.Track;

namespace E2ETests.WebApi;

public class SubmitChordsE2ETests : IClassFixture<E2EFixture>
{
    private readonly E2EFixture _fixture;

    public SubmitChordsE2ETests(E2EFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SubmitChords_WithoutToken_ShouldReturnUnauthorized()
    {
        var track = _fixture.DiscoveredTracks[0];
        var response = await _fixture.SendAsync(HttpMethod.Post, E2EFixture.SubmitChordsEndpoint, BuildRequest(track.TrackId));
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SpotifySearch_ShouldReturnNightmareAlbumTracks()
    {
        _fixture.DiscoveredTracks.Should().NotBeEmpty();
        _fixture.DiscoveredTracks.Should().AllSatisfy(t =>
        {
            t.TrackId.Should().NotBeNullOrEmpty();
            t.Title.Should().NotBeNullOrEmpty();
            t.ArtistName.Should().NotBeNullOrEmpty();
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task SubmitChords_ForDiscoveredTrack_ShouldPersistToRealDynamoDB(int trackIndex)
    {
        var track = _fixture.DiscoveredTracks[trackIndex];
        var request = BuildRequest(track.TrackId);

        var response = await _fixture.SendAuthenticatedAsync(HttpMethod.Post, E2EFixture.SubmitChordsEndpoint, request);

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK,
            $"track[{trackIndex}] = '{track.Title}' (id: {track.TrackId}). Response: {body}");
    }

    private SubmitChordsRequest BuildRequest(string trackId) => new()
    {
        TrackId = trackId,
        ContributorId = "e2e-contributor",
        ContributorName = "E2E Test",
        ContributorEmail = _fixture.TestUserEmail,
        Content =
        [
            new Section
            {
                Type = "Verse",
                Lines =
                [
                    new Line { Lyrics = "I see your fantasy", Chords = { ["0"] = "Em", ["9"] = "C" } },
                    new Line { Lyrics = "You want to make it a reality", Chords = { ["0"] = "G", ["5"] = "D" } }
                ]
            },
            new Section
            {
                Type = "Chorus",
                Lines =
                [
                    new Line { Lyrics = "Nightmare", Chords = { ["0"] = "Em", ["5"] = "Am", ["9"] = "C" } }
                ]
            }
        ]
    };
}
