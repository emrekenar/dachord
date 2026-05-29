namespace Domain.Models.Track;

public class Track
{
    // PK: $"TRACK#{Id}"
    // SK: "METADATA"
    public required string Id { get; set; }
    public required string Title { get; set; }

    // GSI 1: Partition by Artist to get all songs by an artist
    public required string ArtistId { get; set; }
    public required string ArtistName { get; set; }

    // GSI 2: Partition by Album to get all songs in an album
    public required string AlbumId { get; set; }
    public required string AlbumName { get; set; }

    public int TrackNumber { get; set; }  // For sorting within the album

    public string? ImageUrl { get; set; }
    public string? Url { get; set; }
}