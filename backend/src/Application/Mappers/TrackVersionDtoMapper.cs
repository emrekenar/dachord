namespace Application.Mappers;

using Application.Requests;
using Application.Responses;
using Domain.Models.Track;

public static class TrackVersionDtoMapper
{
    public static TrackVersion MapFromRequest(SubmitChordsRequest request)
    {
        return new TrackVersion
        {
            TrackId = request.TrackId,
            ContributorId = request.ContributorId,
            ContributorName = request.ContributorName,
            ContributorEmail = request.ContributorEmail,
            Content = request.Content,
        };
    }

    public static TrackVersionResponse MapToResponse(TrackVersion trackVersion)
    {
        return new TrackVersionResponse
        {
            TrackId = trackVersion.TrackId,
            ContributorId = trackVersion.ContributorId,
            ContributorName = trackVersion.ContributorName,
            ContributorEmail = trackVersion.ContributorEmail,
            IsApproved = trackVersion.IsApproved,
            LikeCount = trackVersion.LikeCount,
            UpdatedAt = trackVersion.UpdatedAt,
            IsUpdated = trackVersion.IsUpdated,
            Content = trackVersion.Content
        };
    }
}