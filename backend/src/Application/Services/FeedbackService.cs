namespace Application.Services;

using Domain.Errors;
using Domain.Interfaces;
using Domain.Models.Feedback;
using Domain.Wrappers;
using Application.Interfaces;

public class FeedbackService(IFeedbackRepository feedbackRepository) : IFeedbackService
{
    private const int MaxLength = 2000;

    public async Task<Result<bool>> ExecuteAsync(string userId, string? email, string message)
    {
        var trimmed = message?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return Result<bool>.Failure(new Error(ErrorCode.InvalidRequest, "Feedback message is required."));
        if (trimmed.Length > MaxLength)
            return Result<bool>.Failure(new Error(ErrorCode.InvalidRequest, "Feedback message is too long."));

        await feedbackRepository.SaveAsync(new Feedback
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Email = email,
            Message = trimmed,
        });
        return Result<bool>.Success(true);
    }
}
