namespace Application.Interfaces;

using Application.Requests;
using Application.Responses;

public interface ISearchChordsService
{
    Task<SearchChordsResponse?> ExecuteAsync(TrackSearchRequest request);
}