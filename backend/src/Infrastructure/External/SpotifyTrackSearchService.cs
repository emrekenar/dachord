namespace Infrastructure.External;

using System.Text.Json;
using Application.Interfaces;
using Application.Models;
using Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

public class SpotifySearchTracksService : ISearchTracksService
{
    private readonly SpotifyOptions _options;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _accessTokenCache;

    public SpotifySearchTracksService(HttpClient httpClient, IOptions<SpotifyOptions> options, IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _accessTokenCache = memoryCache;
    }

    private async Task<string> GetAccessTokenFromCacheAsync()
    {
        if (_accessTokenCache.TryGetValue("SpotifyAccessToken", out string token))
        {
            return token;
        }

        var dict = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _options.ClientId },
            { "client_secret", _options.ClientSecret }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        request.Content = new FormUrlEncodedContent(dict);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(content);

        if (tokenResponse == null)
            throw new Exception("Failed to retrieve Spotify access token.");

        _accessTokenCache.Set("SpotifyAccessToken", tokenResponse.AccessToken, TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 60));

        return tokenResponse.AccessToken;
    }

    public async Task<IResult> ExecuteAsync(TrackSearchRequest request)
    {
        var token = await GetAccessTokenFromCacheAsync();

        var encodedQuery = Uri.EscapeDataString(request.Query);
        var searchUrl = $"{_options.BaseUrl}/v1/search?q={encodedQuery}&type=track&limit=10";

        var spotifyRequest = new HttpRequestMessage(HttpMethod.Get, searchUrl);
        spotifyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var spotifyResponse = await _httpClient.SendAsync(spotifyRequest);
        spotifyResponse.EnsureSuccessStatusCode();

        var content = await spotifyResponse.Content.ReadAsStringAsync();
        var response = MapSpotifyToResponse(content);
        return Results.Ok(response);
    }

    private IEnumerable<TrackResponse> MapSpotifyToResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("tracks").GetProperty("items");

        var results = items.EnumerateArray()
            .Select(item => new TrackResponse
            {
                Id = item.GetProperty("id").GetString()!,
                Title = item.GetProperty("name").GetString()!,
                Artist = item.GetProperty("artists")[0].GetProperty("name").GetString(),
                Url = item.GetProperty("external_urls").GetProperty("spotify").GetString()!
            })
            .ToList();
        return results;
    }
}