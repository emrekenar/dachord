namespace Application.Interfaces;

using Domain.Wrappers;
using Application.Requests;
using Application.Responses;

public interface ISubmitChordsService
{
    Task<Result<TrackVersionResponse>> ExecuteAsync(SubmitChordsRequest request);
}