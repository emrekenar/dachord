namespace Application.Interfaces;

using Domain.Wrappers;
using Application.Requests;
using Application.Responses;

public interface ISubmitChordsService
{
    Task<Result<TrackResponse>> ExecuteAsync(SubmitChordsRequest request);
}