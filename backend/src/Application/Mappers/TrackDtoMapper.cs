namespace Application.Mappers;

using Application.Responses;
using Domain.Models.Track;

public static class TrackDtoMapper
{
    public static TrackResponse MapToResponse(Track track)
    {
        return new TrackResponse
        {
            TrackId = track.Id,
            Title = track.Title,
            ArtistId = track.ArtistId,
            ArtistName = track.ArtistName,
            AlbumId = track.AlbumId,
            AlbumName = track.AlbumName,
        };
    }
}