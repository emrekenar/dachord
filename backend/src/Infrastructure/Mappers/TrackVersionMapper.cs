namespace Infrastructure.Mappers;

using Domain.Models.Track;
using Infrastructure.Entities;

public static class TrackVersionMapper
{
    public static TrackVersion MapToDomainModel(TrackItem entity)
    {
        return new TrackVersion
        {
            TrackId = entity.TrackId,
            ContributorId = entity.ContributorId!,
            ContributorName = entity.ContributorName,
            ContributorEmail = entity.ContributorEmail,
            IsApproved = entity.IsApproved ?? false,
            LikeCount = entity.LikeCount ?? 0,
            UpdatedAt = entity.UpdatedAt ?? DateTime.UtcNow,
            IsUpdated = entity.IsUpdated ?? false,
            ChordsUsed = entity.ChordsUsed ?? [],
            Content = entity.Content ?? [],
        };
    }

    public static TrackItem MapToEntity(TrackVersion trackVersion)
    {
        return new TrackItem
        {
            TrackId = trackVersion.TrackId,
            SortKey = TrackVersionEntity.SK(trackVersion.ContributorId),
            ContributorId = trackVersion.ContributorId,
            ContributorName = trackVersion.ContributorName,
            ContributorEmail = trackVersion.ContributorEmail,
            IsApproved = trackVersion.IsApproved,
            LikeCount = trackVersion.LikeCount,
            UpdatedAt = trackVersion.UpdatedAt,
            IsUpdated = trackVersion.IsUpdated,
            ChordsUsed = trackVersion.ChordsUsed,
            Content = trackVersion.Content,
        };
    }
}