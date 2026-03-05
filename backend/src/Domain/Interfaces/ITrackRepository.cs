namespace Domain.Interfaces;

using Domain.Models;

public interface ITrackRepository
{
    Task SaveAsync(Track track);
    Task<Track?> GetByIdAsync(int id);
    Task<List<Track>> GetAllAsync();
}