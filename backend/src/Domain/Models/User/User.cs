namespace Domain.Models.User;

public class User
{
    // PK: $"USER#{Id}"
    // SK: "METADATA"
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public int NumberOfApprovedSongs { get; set; }
    public int NumberOfLikes { get; set; }
}