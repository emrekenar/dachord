namespace Application.Services;

using Domain.Interfaces;
using Domain.Wrappers;
using Domain.Errors;
using Application.Interfaces;

public class ApproveChordService(ITrackRepository trackRepository) : IApproveChordService
{
    public async Task<Result<bool>> ExecuteAsync(string trackId, string contributorId)
    {
        var version = await trackRepository.GetTrackVersionAsync(trackId, contributorId);
        if (version is null)
            return Result<bool>.Failure(new Error(ErrorCode.TrackNotFound, "Track version not found."));

        version.IsApproved = !version.IsApproved;
        await trackRepository.SaveTrackVersionAsync(version);
        return Result<bool>.Success(true);
    }
}
