namespace Application.Configuration;

public class SpotifyOptions
{
    public const string SectionName = "Spotify";

    public string BaseUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}