namespace Application.Services;

using Domain.Interfaces;
using Domain.Wrappers;
using Application.Interfaces;
using Application.Requests;
using Application.Responses;
using Microsoft.Extensions.Logging;
using Domain.Errors;
using Application.Mappers;

public class SubmitChordsService(
    ITrackRepository trackRepository,
    IUserRepository userRepository,
    ISearchTracksService searchTracksService,
    ILogger<SubmitChordsService> logger) : ISubmitChordsService
{
    public async Task<Result<TrackVersionResponse>> ExecuteAsync(SubmitChordsRequest request)
    {
        var existingTrack = await trackRepository.GetTrackAsync(request.TrackId);
        if (existingTrack == null)
        {
            var spotifyTrack = await searchTracksService.GetTrackAsync(request.TrackId);
            if (spotifyTrack == null)
            {
                logger.LogWarning("Track not found: {TrackId}", request.TrackId);
                return Result<TrackVersionResponse>.Failure(new Error(ErrorCode.TrackNotFound, "Track not found."));
            }
        }

        if (!ValidateRequest(request))
        {
            logger.LogWarning("Chord submission request invalid: {Request}", request);
            return Result<TrackVersionResponse>.Failure(new Error(ErrorCode.InvalidRequest, "Invalid request."));
        }

        var user = await userRepository.GetByIdAsync(request.ContributorId);
        var trackVersion = TrackVersionDtoMapper.MapFromRequest(request, user?.DisplayName);
        await trackRepository.SaveTrackVersionAsync(trackVersion);

        var response = TrackVersionDtoMapper.MapToResponse(trackVersion);
        return Result<TrackVersionResponse>.Success(response);
    }

    private static bool ValidateRequest(SubmitChordsRequest request)
    {
        return request.Content.Count > 0;
    }
}
