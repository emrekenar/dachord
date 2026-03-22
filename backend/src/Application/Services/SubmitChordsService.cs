namespace Application.Services;

using Domain.Interfaces;
using Domain.Models;
using Domain.Wrappers;
using Application.Interfaces;
using Application.Requests;
using Application.Responses;

public class SubmitChordsService(ITrackRepository trackRepository) : ISubmitChordsService
{
    public async Task<Result<TrackResponse>> ExecuteAsync(SubmitChordsRequest request)
    {
        var trackModel = new Track
        {
            Title = request.Title,
            Artist = request.Artist,
            Lyrics = request.Lyrics
        };
        await trackRepository.SaveAsync(trackModel);

        var trackResponse = new TrackResponse
        {
            Id = trackModel.Id,
            Title = trackModel.Title,
            Artist = trackModel.Artist,
            Lyrics = trackModel.Lyrics
        };
        return Result<TrackResponse>.Success(trackResponse);
    }
}