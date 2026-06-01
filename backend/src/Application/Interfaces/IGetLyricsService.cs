namespace Application.Interfaces;

using Application.Responses;
using Domain.Wrappers;

public interface IGetLyricsService
{
    Task<Result<LyricsResponse>> ExecuteAsync(string trackId);
}
