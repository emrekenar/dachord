namespace Application.Interfaces;

using Domain.Wrappers;

public interface IApproveChordService
{
    Task<Result<bool>> ExecuteAsync(string trackId);
}
