namespace Application.Services;

using Application.Interfaces;
using Application.Mappers;
using Application.Requests;
using Application.Responses;
using Domain.Interfaces;
using Domain.Models.Track;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

public class SearchChordsService(ISearchTracksService searchTracksService, ITrackRepository trackRepository, ILogger<SearchChordsService> logger)
    : ISearchChordsService
{
    public async Task<SearchChordsResponse?> ExecuteAsync(TrackSearchRequest request)
    {
        SearchTracksResponse tracks;

        bool isQuerySearch = false;

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
            isQuerySearch = true;
        }

        var response = new SearchChordsResponse();
        var seenTrackIds = new HashSet<string>();

        foreach (TrackResponse track in tracks.Results)
        {
            seenTrackIds.Add(track.TrackId);
            var trackVersions = await trackRepository.GetTrackVersionsAsync(track.TrackId);
            var versionResponses = trackVersions
                .Select(TrackVersionDtoMapper.MapToResponse)
                .ToList();
            response.Results.Add(new TrackVersionsPair(track, versionResponses));
        }

        if (isQuerySearch)
        {
            var artistIds = tracks.Results.Select(t => t.ArtistId).Distinct();
            foreach (var artistId in artistIds)
            {
                var artistTracks = await trackRepository.GetTracksByArtistAsync(artistId);
                foreach (var artistTrack in artistTracks)
                {
                    if (seenTrackIds.Contains(artistTrack.Id)) continue;
                    seenTrackIds.Add(artistTrack.Id);
                    var versions = await trackRepository.GetTrackVersionsAsync(artistTrack.Id);
                    var versionResponses = versions.Select(TrackVersionDtoMapper.MapToResponse).ToList();
                    if (versionResponses.Count > 0)
                        response.Results.Add(new TrackVersionsPair(TrackDtoMapper.MapToResponse(artistTrack), versionResponses));
                }
            }
        }

        logger.LogInformation("SearchChordsService returning response of length {Length}", response.Results.Count);

        return response;
    }
}
