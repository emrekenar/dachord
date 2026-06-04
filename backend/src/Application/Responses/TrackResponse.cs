namespace Application.Responses;

public class TrackResponse
{
    public required string TrackId { get; set; }
    public required string Title { get; set; }
    public required string ArtistId { get; set; }
    public required string ArtistName { get; set; }
    public required string AlbumId { get; set; }
    public required string AlbumName { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ReleaseYear { get; set; }
}