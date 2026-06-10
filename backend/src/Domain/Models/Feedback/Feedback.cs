namespace Domain.Models.Feedback;

public class Feedback
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public string? Email { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
