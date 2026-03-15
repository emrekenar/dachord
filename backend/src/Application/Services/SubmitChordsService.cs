namespace Application.Services;

using Application.Interfaces;
using Application.Models;
using Domain.Interfaces;
using Domain.Models;

public class SubmitChordsService(ITrackRepository trackRepository) : ISubmitChordsService
{
    public async Task<IResult> ExecuteAsync(SubmitChordsRequest request)
    {
        var trackModel = new Track
        {
            Title = request.Title,
            Artist = request.Artist,
            Lyrics = request.Lyrics
        };
        await trackRepository.SaveAsync(trackModel);
        return Results.Created($"/tracks/{trackModel.Id}", trackModel);
    }
}