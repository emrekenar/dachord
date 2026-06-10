namespace Domain.Interfaces;

using Domain.Models.Track;

public interface ITrackRepository
{
    Task SaveTrackAsync(Track track);
    Task<Track?> GetTrackAsync(string id);

    Task SaveTrackVersionAsync(TrackVersion trackVersion);
    Task<TrackVersion?> GetTrackVersionAsync(string id);
    Task<TrackVersion?> GetTrackVersionAsync(string trackId, string contributorId);
    Task<IEnumerable<TrackVersion>> GetTrackVersionsAsync(string trackId);
    Task<IEnumerable<Track>> GetTracksByArtistAsync(string artistId);
    Task<IEnumerable<TrackVersion>> GetPendingPopularVersionsAsync(int minLikes);
}