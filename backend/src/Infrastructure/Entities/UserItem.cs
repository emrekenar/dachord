namespace Infrastructure.Entities;

using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("users")]
public class UserItem
{
    [DynamoDBHashKey("pk")]
    public required string Id { get; set; }
 
    [DynamoDBProperty]
    public required string Email { get; set; }

    [DynamoDBProperty]
    public required string PasswordHash { get; set; }

    [DynamoDBProperty]
    public string DisplayName { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Bio { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string AvatarIcon { get; set; } = string.Empty;

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
    Moderator,

    [DynamoDBProperty("Admin")]
    Admin
}

public static class UserEntity
{
    public static string PK(string id) => $"USER#{id}";
    public static string SK => "METADATA";
}