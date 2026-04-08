namespace Domain.Models.User;

public class TrackLike
{
    // PK: TRACK#{TrackId}
    public required string TrackId { get; set; }

    // SK: LIKE#{UserId}
    public required string UserId { get; set; }

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
}