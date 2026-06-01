namespace Infrastructure.External;

using System.Net.Http.Headers;
using System.Text.Json;
using Application.Configuration;
using Application.Interfaces;
using Domain.Models.Track;
using Microsoft.Extensions.Options;

public class GeniusLyricsService : ILyricsService
{
    private readonly HttpClient _httpClient;
    private readonly GeniusOptions _options;

    public GeniusLyricsService(IHttpClientFactory httpClientFactory, IOptions<GeniusOptions> options)
    {
        _httpClient = httpClientFactory.CreateClient("Genius");
        _options = options.Value;
    }

    public async Task<List<Section>?> GetLyricsAsync(string artist, string title)
    {
        var lyricsUrl = await SearchAsync(artist, title);
        if (lyricsUrl == null)
            return null;

        var pageResponse = await _httpClient.GetAsync(lyricsUrl);
        if (!pageResponse.IsSuccessStatusCode)
            return null;

        var html = await pageResponse.Content.ReadAsStringAsync();
        return GeniusHtmlParser.Parse(html);
    }

    private async Task<string?> SearchAsync(string artist, string title)
    {
        var query = Uri.EscapeDataString($"{artist} {title}");
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.genius.com/search?q={query}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        return ParseSearchResponse(json, artist);
    }

    private static string? ParseSearchResponse(string json, string artist)
    {
        using var doc = JsonDocument.Parse(json);
        var hits = doc.RootElement.GetProperty("response").GetProperty("hits");

        foreach (var hit in hits.EnumerateArray())
        {
            var result = hit.GetProperty("result");
            var primaryArtist = result.GetProperty("primary_artist").GetProperty("name").GetString() ?? "";
            if (primaryArtist.Contains(artist, StringComparison.OrdinalIgnoreCase)
                || artist.Contains(primaryArtist, StringComparison.OrdinalIgnoreCase))
            {
                return result.GetProperty("url").GetString();
            }
        }

        return null;
    }
}
