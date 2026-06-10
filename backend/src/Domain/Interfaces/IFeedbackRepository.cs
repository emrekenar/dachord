namespace Domain.Interfaces;

using Domain.Models.Feedback;

public interface IFeedbackRepository
{
    Task SaveAsync(Feedback feedback);
}
