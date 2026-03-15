namespace Application.Interfaces;

using Application.Models;

public interface ISubmitChordsService
{
    Task<IResult> ExecuteAsync(SubmitChordsRequest request);
}