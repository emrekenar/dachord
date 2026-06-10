namespace Application.Interfaces;

using Domain.Wrappers;

public interface IFeedbackService
{
    Task<Result<bool>> ExecuteAsync(string userId, string? email, string message);
}
