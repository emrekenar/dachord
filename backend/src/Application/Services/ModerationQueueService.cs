namespace Application.Services;

using Application.Interfaces;
using Application.Responses;
using Domain.Interfaces;

public class ModerationQueueService(ITrackRepository trackRepository) : IModerationQueueService
{
    public async Task<IReadOnlyList<ModerationQueueItemResponse>> ExecuteAsync(int minLikes)
    {
        var versions = await trackRepository.GetPendingPopularVersionsAsync(minLikes);

        var items = new List<ModerationQueueItemResponse>();
        foreach (var version in versions)
        {
            var track = await trackRepository.GetTrackAsync(version.TrackId);
            if (track is null) continue;

            items.Add(new ModerationQueueItemResponse
            {
                TrackId = version.TrackId,
                Title = track.Title,
                ArtistName = track.ArtistName,
                ImageUrl = track.ImageUrl,
                ContributorId = version.ContributorId,
                ContributorName = version.ContributorName,
                LikeCount = version.LikeCount,
            });
        }

        return items;
    }
}
