namespace Infrastructure.Persistence;

using Amazon.DynamoDBv2.DataModel;

using Domain.Interfaces;
using Domain.Models.Track;
using Infrastructure.Entities;
using Infrastructure.Mappers;

public class TrackRepository : ITrackRepository
{
    private readonly IDynamoDBContext _dynamoDbContext;

    public TrackRepository(IDynamoDBContext dynamoDbContext)
    {
        _dynamoDbContext = dynamoDbContext;
    }

    public async Task SaveTrackAsync(Track track)
    {
        var item = TrackMapper.MapToEntity(track);
        await _dynamoDbContext.SaveAsync(item);
    }

    public async Task<Track?> GetTrackAsync(string id)
    {
        var response = await _dynamoDbContext.LoadAsync<TrackItem>(id);
        if (response == null)
            return null;
        return TrackMapper.MapToDomainModel(response);
    }

    public async Task SaveTrackVersionAsync(TrackVersion trackVersion)
    {
        var item = TrackVersionMapper.MapToEntity(trackVersion);
        await _dynamoDbContext.SaveAsync(item);
    }

    public async Task<TrackVersion?> GetTrackVersionAsync(string id)
    {
        var response = await _dynamoDbContext.LoadAsync<TrackItem>(id);
        if (response == null)
            return null;
        return TrackVersionMapper.MapToDomainModel(response);
    }
}