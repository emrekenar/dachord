namespace Infrastructure.Entities;

using Amazon.DynamoDBv2.DataModel;
using Domain.Models.Track;


[DynamoDBTable("Tracks")]
public class TrackItem
{
    [DynamoDBHashKey]
    public required string TrackId { get; set; }

    [DynamoDBRangeKey("SK")]
    public string SortKey { get; set; } = string.Empty;

    // Track fields
    [DynamoDBProperty]
    public string? Title { get; set; }

    [DynamoDBProperty]
    public string? ArtistId { get; set; }

    [DynamoDBProperty]
    public string? ArtistName { get; set; }

    [DynamoDBProperty]
    public string? AlbumId { get; set; }

    [DynamoDBProperty]
    public string? AlbumName { get; set; }

    [DynamoDBProperty]
    public int? TrackNumber { get; set; }

    // TrackVersion fields
    [DynamoDBProperty]
    public string? ContributorId { get; set; }

    [DynamoDBProperty]
    public string? ContributorName { get; set; }

    [DynamoDBProperty]
    public string? ContributorEmail { get; set; }

    [DynamoDBProperty]
    public bool? IsApproved { get; set; }

    [DynamoDBProperty]
    public int? LikeCount { get; set; }

    [DynamoDBProperty]
    public DateTime? UpdatedAt { get; set; }

    [DynamoDBProperty]
    public bool? IsUpdated { get; set; }

    [DynamoDBProperty]
    public List<string>? ChordsUsed { get; set; }

    [DynamoDBProperty]
    public List<Section>? Content { get; set; }
}

public static class TrackEntity
{
    public static string PK(string id) => $"TRACK#{id}";
    public static string SK => "METADATA";
}

public static class TrackVersionEntity
{
    public static string PK(string trackId) => $"TRACK#{trackId}";
    public static string SK(string contributorId) => $"USER#{contributorId}";
}