namespace Domain.Errors;

public record Error(ErrorCode Code, string? Description) {}