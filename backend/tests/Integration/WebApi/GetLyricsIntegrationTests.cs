namespace IntegrationTests.WebApi;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces;
using Application.Requests;
using Application.Responses;
using Domain.Errors;
using Domain.Models.Track;
using IntegrationTests.TestInfrastructure;

public class GetLyricsIntegrationTests : IClassFixture<IntegrationFixture>
{
    private const string TrackId = "test-lyrics-track";

    private const string SamplePlainLyrics = """
        [Verse 1]
        First line of the verse
        Second line of the verse

        [Chorus]
        First chorus line
        Second chorus line
        """;

    private readonly FakeLrclibHttpMessageHandler _lrclibHandler = new();
    private readonly HttpClient _client;

    public GetLyricsIntegrationTests(IntegrationFixture fixture)
    {
        _lrclibHandler.SetupLyrics(SamplePlainLyrics);

        // StubSearchTracksService returns a track only for TrackId — null for anything else.
        // FakeLrclibHttpMessageHandler intercepts all lrclib HTTP traffic so real parsing runs.
        var factory = fixture.Factory!.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<ISearchTracksService>(
                    new StubSearchTracksService(TrackId, "Test Artist", "Test Song"));
                services.AddHttpClient("Lrclib")
                    .ConfigurePrimaryHttpMessageHandler(() => _lrclibHandler);
            });
        });

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetLyrics_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync($"/tracks/{TrackId}/lyrics");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLyrics_ValidRequest_Returns200WithSections()
    {
        var token = await GetTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/tracks/{TrackId}/lyrics");
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var sections = body.GetProperty("sections");
        sections.GetArrayLength().Should().BeGreaterThan(0);

        var firstSection = sections[0];
        firstSection.GetProperty("lines").GetArrayLength().Should().BeGreaterThan(0);

        var firstLine = firstSection.GetProperty("lines")[0];
        firstLine.GetProperty("lyrics").GetString().Should().NotBeNullOrEmpty();
        firstLine.GetProperty("chords").EnumerateObject().Should().BeEmpty();
    }

    [Fact]
    public async Task GetLyrics_WhenLrclibReturns404_Returns404WithLyricsNotFound()
    {
        _lrclibHandler.SetupNotFound();
        var token = await GetTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/tracks/{TrackId}/lyrics");
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("code").GetInt32().Should().Be((int)ErrorCode.LyricsNotFound);
    }

    [Fact]
    public async Task GetLyrics_WhenTrackNotFound_Returns404WithTrackNotFound()
    {
        // StubSearchTracksService returns null for any ID other than TrackId
        var token = await GetTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/tracks/unknown-id/lyrics");
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("code").GetInt32().Should().Be((int)ErrorCode.TrackNotFound);
    }

    private async Task<string> GetTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/login", new LoginRequest
        {
            Email = IntegrationFixture.ExistingTestUserEmail,
            Password = "TestPassword123!"
        });
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }
}

file sealed class StubSearchTracksService(string trackId, string artistName, string title) : ISearchTracksService
{
    public Task<SearchTracksResponse> QueryAsync(TrackSearchRequest request) => Task.FromResult(new SearchTracksResponse([]));
    public Task<Track?> GetTrackAsync(string id) =>
        Task.FromResult<Track?>(id == trackId
            ? new Track { Id = id, Title = title, ArtistName = artistName, ArtistId = "a1", AlbumId = "al1", AlbumName = "Album" }
            : null);
    public Task<SearchTracksResponse> GetTracksInAlbum(string albumId) => Task.FromResult(new SearchTracksResponse([]));
    public Task<SearchTracksResponse> GetTracksFromArtist(string artistId) => Task.FromResult(new SearchTracksResponse([]));
    public Task<List<AlbumResponse>> GetArtistAlbumsAsync(string artistId) => Task.FromResult(new List<AlbumResponse>());
}
