namespace Application.Interfaces;

using Microsoft.AspNetCore.Http;
using Application.Requests;

public interface ISubmitChordsService
{
    Task<IResult> ExecuteAsync(SubmitChordsRequest request);
}