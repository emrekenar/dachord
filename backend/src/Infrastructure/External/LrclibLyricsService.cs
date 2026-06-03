namespace Infrastructure.External;

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Application.Interfaces;
using Domain.Models.Track;

public class LrclibLyricsService(IHttpClientFactory httpClientFactory) : ILyricsService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Lrclib");

    public async Task<List<Section>?> GetLyricsAsync(string artist, string title)
    {
        var artistEnc = Uri.EscapeDataString(artist);
        var titleEnc = Uri.EscapeDataString(title);
        var response = await _httpClient.GetAsync(
            $"https://lrclib.net/api/get?artist_name={artistEnc}&track_name={titleEnc}");

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<LrclibResponse>();
        if (result is null) return null;

        if (result.SyncedLyrics is not null)
            return LrclibLyricsParser.ParseSynced(result.SyncedLyrics);

        if (result.PlainLyrics is not null)
            return LrclibLyricsParser.Parse(result.PlainLyrics);

        return null;
    }

    private sealed record LrclibResponse(
        [property: JsonPropertyName("plainLyrics")] string? PlainLyrics,
        [property: JsonPropertyName("syncedLyrics")] string? SyncedLyrics
    );
}
