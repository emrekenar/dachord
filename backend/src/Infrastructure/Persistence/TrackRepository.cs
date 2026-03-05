namespace Infrastructure.Persistence;

using Domain.Models;
using Domain.Interfaces;

public class TrackRepository : ITrackRepository
{
    private readonly Dictionary<int, Track> tracks = new();

    public Task SaveAsync(Track track)
    {
        tracks[track.Id] = track;
        return Task.CompletedTask;
    }

    public Task<Track?> GetByIdAsync(int id)
    {
        tracks.TryGetValue(id, out var track);
        return Task.FromResult(track);
    }

    public Task<List<Track>> GetAllAsync()
    {
        return Task.FromResult(tracks.Values.ToList());
    }
}