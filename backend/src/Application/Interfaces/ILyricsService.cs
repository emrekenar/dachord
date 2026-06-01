namespace Application.Interfaces;

using Domain.Models.Track;

public interface ILyricsService
{
    Task<List<Section>?> GetLyricsAsync(string artist, string title);
}
