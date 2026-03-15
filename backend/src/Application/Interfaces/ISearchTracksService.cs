namespace Application.Interfaces;

using Application.Models;

public interface ISearchTracksService
{
    Task<IResult> ExecuteAsync(TrackSearchRequest request);
}