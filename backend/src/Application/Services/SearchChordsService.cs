namespace Application.Services;

using Application.Interfaces;
using Application.Mappers;
using Application.Requests;
using Application.Responses;
using Domain.Interfaces;
using Domain.Models.Track;
using Microsoft.Extensions.Logging;

public class SearchChordsService(ISearchTracksService searchTracksService, ITrackRepository trackRepository, ILogger<SearchChordsService> logger)
    : ISearchChordsService
{
    public async Task<SearchChordsResponse?> ExecuteAsync(TrackSearchRequest request)
    {
        SearchTracksResponse tracks;

        if (request.ArtistId is not null)
        {
            tracks = await searchTracksService.GetTracksFromArtist(request.ArtistId);
        }
        else if (request.AlbumId is not null)
        {
            tracks = await searchTracksService.GetTracksInAlbum(request.AlbumId);
        }
        else
        {
            if (request.Query.Length <= 2)
            {
                logger.LogWarning("Invalid SearchChords query: {Query}. Query must be at least 2 characters long", request.Query);
                return null;
            }
            tracks = await searchTracksService.QueryAsync(request);
        }

        var response = new SearchChordsResponse();

        foreach (TrackResponse track in tracks.Results)
        {
            var trackVersions = await trackRepository.GetTrackVersionsAsync(track.TrackId);
            var versionResponses = trackVersions
                .Select(TrackVersionDtoMapper.MapToResponse)
                .ToList();
            response.Results.Add(new TrackVersionsPair(track, versionResponses));
        }

        logger.LogInformation("SearchChordsService returning response of length {Length}", response.Results.Count);

        return response;
    }
}
