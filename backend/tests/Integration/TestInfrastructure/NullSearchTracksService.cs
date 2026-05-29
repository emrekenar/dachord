using Application.Interfaces;
using Application.Requests;
using Application.Responses;
using Domain.Models.Track;

namespace IntegrationTests.TestInfrastructure;

internal class NullSearchTracksService : ISearchTracksService
{
    public Task<SearchTracksResponse> QueryAsync(TrackSearchRequest request) => Task.FromResult(new SearchTracksResponse([]));
    public Task<Track?> GetTrackAsync(string id) => Task.FromResult<Track?>(null);
    public Task<SearchTracksResponse> GetTracksInAlbum(string albumId) => Task.FromResult(new SearchTracksResponse([]));
    public Task<SearchTracksResponse> GetTracksFromArtist(string artistId) => Task.FromResult(new SearchTracksResponse([]));
    public Task<List<AlbumResponse>> GetArtistAlbumsAsync(string artistId) => Task.FromResult(new List<AlbumResponse>());
}
