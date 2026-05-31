namespace Application.Requests;

public class TrackSearchRequest
{
    public string Query { get; set; } = string.Empty;
    public string? ArtistId { get; set; }
    public string? AlbumId { get; set; }
}