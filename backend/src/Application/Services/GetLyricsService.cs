namespace Application.Services;

using Application.Interfaces;
using Application.Responses;
using Domain.Errors;
using Domain.Wrappers;

public class GetLyricsService(
    ISearchTracksService searchTracksService,
    ILyricsService lyricsService) : IGetLyricsService
{
    public async Task<Result<LyricsResponse>> ExecuteAsync(string trackId)
    {
        var track = await searchTracksService.GetTrackAsync(trackId);
        if (track == null)
            return Result<LyricsResponse>.Failure(new Error(ErrorCode.TrackNotFound, "Track not found."));

        var sections = await lyricsService.GetLyricsAsync(track.ArtistName, track.Title);
        if (sections == null)
            return Result<LyricsResponse>.Failure(new Error(ErrorCode.LyricsNotFound, "Lyrics not found."));

        return Result<LyricsResponse>.Success(new LyricsResponse(sections));
    }
}
