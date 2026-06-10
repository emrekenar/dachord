namespace Application.Responses;

public class ModerationQueueItemResponse
{
    public required string TrackId { get; set; }
    public required string Title { get; set; }
    public required string ArtistName { get; set; }
    public string? ImageUrl { get; set; }

    public required string ContributorId { get; set; }
    public string? ContributorName { get; set; }
    public int LikeCount { get; set; }
}
