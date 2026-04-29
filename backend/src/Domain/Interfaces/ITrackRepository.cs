namespace Domain.Interfaces;

using Domain.Models.Track;

public interface ITrackRepository
{
    Task SaveTrackAsync(Track track);
    Task<Track?> GetTrackAsync(string id);

    Task SaveTrackVersionAsync(TrackVersion trackVersion);
    Task<TrackVersion?> GetTrackVersionAsync(string id);
    Task<IEnumerable<TrackVersion>> GetTrackVersionsAsync(string trackId);
}