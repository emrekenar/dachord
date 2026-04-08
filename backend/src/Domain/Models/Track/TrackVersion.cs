namespace Domain.Models.Track;

public class TrackVersion
{
    // PK: TRACK#{TrackId}
    public required string TrackId { get; set; }

    // SK: USER#{ContributorId}
    public required string ContributorId { get; set; }
    public string? ContributorName { get; set; }
    public string? ContributorEmail { get; set; }

    public bool IsApproved { get; set; } = false;
    public int LikeCount { get; set; } = 0;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsUpdated { get; set; } = false;

    public List<string> ChordsUsed { get; set; } = new();  // for analysis

    public List<Section> Content { get; set; } = new();
}