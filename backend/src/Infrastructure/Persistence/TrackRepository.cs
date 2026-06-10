namespace Infrastructure.Persistence;

using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

using Domain.Interfaces;
using Domain.Models.Track;
using Infrastructure.Entities;
using Infrastructure.Mappers;

public class TrackRepository(IDynamoDBContext dynamoDbContext) : ITrackRepository
{
    public async Task SaveTrackAsync(Track track)
    {
        var item = TrackMapper.MapToEntity(track);
        await dynamoDbContext.SaveAsync(item);
    }

    public async Task<Track?> GetTrackAsync(string id)
    {
        var response = await dynamoDbContext.LoadAsync<TrackItem>(id, TrackEntity.SK);
        if (response == null)
            return null;
        return TrackMapper.MapToDomainModel(response);
    }

    public async Task SaveTrackVersionAsync(TrackVersion trackVersion)
    {
        var item = TrackVersionMapper.MapToEntity(trackVersion);
        await dynamoDbContext.SaveAsync(item);
    }

    public async Task<TrackVersion?> GetTrackVersionAsync(string id)
    {
        var versions = await GetTrackVersionsAsync(id);
        return versions.FirstOrDefault();
    }

    public async Task<TrackVersion?> GetTrackVersionAsync(string trackId, string contributorId)
    {
        var item = await dynamoDbContext.LoadAsync<TrackItem>(trackId, TrackVersionEntity.SK(contributorId));
        return item is null ? null : TrackVersionMapper.MapToDomainModel(item);
    }

    public async Task<IEnumerable<TrackVersion>> GetTrackVersionsAsync(string trackId)
    {
        var search = dynamoDbContext.QueryAsync<TrackItem>(
            trackId,
            QueryOperator.BeginsWith,
            ["USER#"]
        );
        var items = await search.GetRemainingAsync();
        return items.Select(TrackVersionMapper.MapToDomainModel);
    }

    public async Task<IEnumerable<Track>> GetTracksByArtistAsync(string artistId)
    {
        var scan = dynamoDbContext.ScanAsync<TrackItem>(new List<ScanCondition>
        {
            new ScanCondition("ArtistId", ScanOperator.Equal, artistId),
            new ScanCondition("SortKey", ScanOperator.Equal, TrackEntity.SK),
        });
        var items = await scan.GetRemainingAsync();
        return items.Select(TrackMapper.MapToDomainModel);
    }

    public async Task<IEnumerable<TrackVersion>> GetPendingPopularVersionsAsync(int minLikes)
    {
        var scan = dynamoDbContext.ScanAsync<TrackItem>(new List<ScanCondition>
        {
            new ScanCondition("SortKey", ScanOperator.BeginsWith, "USER#"),
            new ScanCondition("IsApproved", ScanOperator.Equal, false),
            new ScanCondition("LikeCount", ScanOperator.GreaterThanOrEqual, minLikes),
        });
        var items = await scan.GetRemainingAsync();
        return items
            .Select(TrackVersionMapper.MapToDomainModel)
            .OrderByDescending(v => v.LikeCount);
    }
}
