namespace Infrastructure.Mappers;

using Domain.Models.Track;
using Infrastructure.Entities;

public static class TrackMapper
{
    public static Track MapToDomainModel(TrackItem entity)
    {
        return new Track
        {
            Id = entity.TrackId,
            Title = entity.Title!,
            ArtistId = entity.ArtistId!,
            ArtistName = entity.ArtistName!,
            AlbumId = entity.AlbumId!,
            AlbumName = entity.AlbumName!,
            TrackNumber = entity.TrackNumber ?? 0,
        };
    }

    public static TrackItem MapToEntity(Track track)
    {
        return new TrackItem
        {
            TrackId = track.Id,
            SortKey = TrackEntity.SK,
            Title = track.Title,
            ArtistId = track.ArtistId,
            ArtistName = track.ArtistName,
            AlbumId = track.AlbumId,
            AlbumName = track.AlbumName,
            TrackNumber = track.TrackNumber,
        };
    }
}