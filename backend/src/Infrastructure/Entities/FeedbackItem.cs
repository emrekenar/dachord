namespace Infrastructure.Entities;

using Amazon.DynamoDBv2.DataModel;

// Stored in the shared "tracks" table (single-table design) under a dedicated
// FEEDBACK partition so feedback never collides with track or version items.
[DynamoDBTable("tracks")]
public class FeedbackItem
{
    [DynamoDBHashKey("pk")]
    public string Pk { get; set; } = FeedbackEntity.PK;

    [DynamoDBRangeKey("sk")]
    public required string Sk { get; set; }

    [DynamoDBProperty]
    public required string UserId { get; set; }

    [DynamoDBProperty]
    public string? Email { get; set; }

    [DynamoDBProperty]
    public required string Message { get; set; }

    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; }
}

public static class FeedbackEntity
{
    public const string PK = "FEEDBACK";
    public static string SK(DateTime createdAt, string id) => $"{createdAt:o}#{id}";
}
