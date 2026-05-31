using Application.Requests;
using Application.Responses;
using Domain.Models.Track;

namespace Application.Interfaces;

public record SearchTracksResponse(List<TrackResponse> Results);

// internal service for searching tracks
// currently provided by Spotify API
public interface ISearchTracksService
{
    Task<SearchTracksResponse> QueryAsync(TrackSearchRequest request);
    Task<Track?> GetTrackAsync(string id);
    Task<SearchTracksResponse> GetTracksInAlbum(string albumId);
    Task<SearchTracksResponse> GetTracksFromArtist(string artistId);
    Task<List<AlbumResponse>> GetArtistAlbumsAsync(string artistId);
}