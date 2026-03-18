namespace Application.Interfaces;

using Microsoft.AspNetCore.Http;
using Application.Requests;

public interface ISearchTracksService
{
    Task<IResult> ExecuteAsync(TrackSearchRequest request);
}