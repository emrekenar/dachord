namespace Application.Responses;

public class AlbumResponse
{
    public required string AlbumId { get; set; }
    public required string AlbumName { get; set; }
    public string? ImageUrl { get; set; }
    public string? ReleaseYear { get; set; }
}
