namespace Infrastructure.Entities;

using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("Users")]
public class UserItem
{
    [DynamoDBHashKey]
    public required string Id { get; set; }
 
    [DynamoDBProperty]
    public required string Email { get; set; }

    [DynamoDBProperty]
    public required string PasswordHash { get; set; }

    [DynamoDBProperty]
    public string DisplayName { get; set; } = string.Empty;

    [DynamoDBProperty]
    public UserRoleEnum Role { get; set; } = UserRoleEnum.User;

    [DynamoDBProperty]
    public int NumberOfApprovedSongs { get; set; }

    [DynamoDBProperty]
    public int NumberOfLikes { get; set; }
}

public enum UserRoleEnum
{
    [DynamoDBProperty("User")]
    User,

    [DynamoDBProperty("Moderator")]
    Moderator
}

public static class UserEntity
{
    public static string PK(string id) => $"USER#{id}";
    public static string SK => "METADATA";
}