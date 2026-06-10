namespace Application.Interfaces;

using Application.Responses;

public interface IModerationQueueService
{
    Task<IReadOnlyList<ModerationQueueItemResponse>> ExecuteAsync(int minLikes);
}
