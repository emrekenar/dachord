namespace Application.Interfaces;

using Domain.Wrappers;
using Application.Requests;
using Application.Responses;

public interface ISearchTracksService
{
    Task<Result<IEnumerable<TrackResponse>>> ExecuteAsync(TrackSearchRequest request);
}