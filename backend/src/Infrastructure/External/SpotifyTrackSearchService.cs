namespace Infrastructure.External;

using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using Application.Interfaces;
using Application.Requests;
using Application.Responses;
using Application.Configuration;
using Infrastructure.Configuration;
using Domain.Models.Track;

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
        if (_accessTokenCache.TryGetValue("SpotifyAccessToken", out string? token) && token != null)
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

    public async Task<SearchTracksResponse> QueryAsync(TrackSearchRequest request)
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
        return response;
    }

    public async Task<Track?> GetTrackAsync(string id)
    {
        var cacheKey = $"Track:{id}";
        if (_accessTokenCache.TryGetValue(cacheKey, out Track? cached))
            return cached;

        var token = await GetAccessTokenFromCacheAsync();
        var url = $"{_options.BaseUrl}/v1/tracks/{id}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        var album = root.GetProperty("album");
        var images = album.GetProperty("images");
        var imageUrl = images.GetArrayLength() > 0 ? images[0].GetProperty("url").GetString() : null;
        var releaseDate = album.TryGetProperty("release_date", out var rd) ? rd.GetString() : null;

        var track = new Track
        {
            Id = root.GetProperty("id").GetString()!,
            Title = root.GetProperty("name").GetString()!,
            ArtistId = root.GetProperty("artists")[0].GetProperty("id").GetString()!,
            ArtistName = root.GetProperty("artists")[0].GetProperty("name").GetString()!,
            AlbumId = album.GetProperty("id").GetString()!,
            AlbumName = album.GetProperty("name").GetString()!,
            ImageUrl = imageUrl,
            Url = root.GetProperty("external_urls").GetProperty("spotify").GetString(),
            ReleaseYear = releaseDate?.Length >= 4 ? releaseDate[..4] : releaseDate,
        };

        _accessTokenCache.Set(cacheKey, track, TimeSpan.FromHours(24));
        return track;
    }

    public async Task<SearchTracksResponse> GetTracksInAlbum(string albumId)
    {
        var token = await GetAccessTokenFromCacheAsync();
        var url = $"{_options.BaseUrl}/v1/albums/{albumId}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return new SearchTracksResponse([]);

        var content = await response.Content.ReadAsStringAsync();
        return MapAlbumToResponse(content, albumId);
    }

    public async Task<SearchTracksResponse> GetTracksFromArtist(string artistId)
    {
        var token = await GetAccessTokenFromCacheAsync();
        var url = $"{_options.BaseUrl}/v1/artists/{artistId}/top-tracks";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return new SearchTracksResponse([]);

        var content = await response.Content.ReadAsStringAsync();
        return MapArtistTopTracksToResponse(content);
    }

    // Shape: { "tracks": { "items": [...] } }  — text search endpoint
    private SearchTracksResponse MapSpotifyToResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("tracks").GetProperty("items");
        return new SearchTracksResponse(MapTrackItems(items));
    }

    // Shape: { "tracks": [...] }  — artist top-tracks endpoint (tracks is an array, not an object)
    private SearchTracksResponse MapArtistTopTracksToResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("tracks");
        return new SearchTracksResponse(MapTrackItems(items));
    }

    // Shape: { "id": "...", "name": "...", "images": [...], "tracks": { "items": [...] } }  — album endpoint
    private SearchTracksResponse MapAlbumToResponse(string json, string albumId)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var albumName = root.GetProperty("name").GetString()!;
        var images = root.GetProperty("images");
        var imageUrl = images.GetArrayLength() > 0 ? images[0].GetProperty("url").GetString() : null;
        var albumRd = root.TryGetProperty("release_date", out var albumRdProp) ? albumRdProp.GetString() : null;
        var albumYear = albumRd?.Length >= 4 ? albumRd[..4] : albumRd;
        var items = root.GetProperty("tracks").GetProperty("items");

        var results = items.EnumerateArray()
            .Select(item => new TrackResponse
            {
                TrackId = item.GetProperty("id").GetString()!,
                Title = item.GetProperty("name").GetString()!,
                ArtistId = item.GetProperty("artists")[0].GetProperty("id").GetString()!,
                ArtistName = item.GetProperty("artists")[0].GetProperty("name").GetString()!,
                AlbumId = albumId,
                AlbumName = albumName,
                Url = item.TryGetProperty("external_urls", out var urls)
                    ? urls.GetProperty("spotify").GetString()!
                    : string.Empty,
                ReleaseYear = albumYear,
            })
            .ToList();

        return new SearchTracksResponse(results);
    }

    public async Task<List<AlbumResponse>> GetArtistAlbumsAsync(string artistId)
    {
        var token = await GetAccessTokenFromCacheAsync();
        var url = $"{_options.BaseUrl}/v1/artists/{artistId}/albums?include_groups=album,single&limit=50";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return [];

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var items = doc.RootElement.GetProperty("items");

        return items.EnumerateArray()
            .Select(item =>
            {
                var images = item.GetProperty("images");
                var imageUrl = images.GetArrayLength() > 0 ? images[0].GetProperty("url").GetString() : null;
                var releaseDate = item.GetProperty("release_date").GetString();
                return new AlbumResponse
                {
                    AlbumId = item.GetProperty("id").GetString()!,
                    AlbumName = item.GetProperty("name").GetString()!,
                    ImageUrl = imageUrl,
                    ReleaseYear = releaseDate?.Length >= 4 ? releaseDate[..4] : releaseDate,
                };
            })
            .ToList();
    }

    private static List<TrackResponse> MapTrackItems(JsonElement items) =>
        items.EnumerateArray()
            .Select(item =>
            {
                var album = item.GetProperty("album");
                var rd = album.TryGetProperty("release_date", out var rdProp) ? rdProp.GetString() : null;
                return new TrackResponse
                {
                    TrackId = item.GetProperty("id").GetString()!,
                    Title = item.GetProperty("name").GetString()!,
                    ArtistId = item.GetProperty("artists")[0].GetProperty("id").GetString()!,
                    ArtistName = item.GetProperty("artists")[0].GetProperty("name").GetString()!,
                    AlbumId = album.GetProperty("id").GetString()!,
                    AlbumName = album.GetProperty("name").GetString()!,
                    Url = item.GetProperty("external_urls").GetProperty("spotify").GetString()!,
                    ReleaseYear = rd?.Length >= 4 ? rd[..4] : rd,
                };
            })
            .ToList();
}