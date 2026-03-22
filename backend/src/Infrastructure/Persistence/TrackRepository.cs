namespace Infrastructure.Persistence;

using System.Text.Json;
using Microsoft.Extensions.Options;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

using Domain.Models;
using Domain.Interfaces;
using Infrastructure.Configuration;

public class TrackRepository : ITrackRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbOptions _options;

    public TrackRepository(IAmazonDynamoDB dynamoDb, IOptions<DynamoDbOptions> options)
    {
        _dynamoDb = dynamoDb;
        _options = options.Value;
    }

    public async Task SaveAsync(Track track)
    {
        var id = string.IsNullOrEmpty(track.Id) ? Guid.NewGuid().ToString() : track.Id;

        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = id },
            ["Type"] = new AttributeValue { S = "Track" },
            ["Title"] = new AttributeValue { S = track.Title },
            ["Artist"] = new AttributeValue { S = track.Artist },
            ["Lyrics"] = new AttributeValue { S = JsonSerializer.Serialize(track.Lyrics) }
        };
        await _dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = _options.TableName,
            Item = item
        });
        track.Id = id;
    }

    public async Task<Track?> GetByIdAsync(string id)
    {
        var response = await _dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = _options.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id },
                ["Type"] = new AttributeValue { S = "Track" }
            }
        });
        if (response.Item == null || response.Item.Count == 0)
            return null;
        return MapTrack(response.Item);
    }

    public async Task<List<Track>> GetAllAsync()
    {
        var response = await _dynamoDb.ScanAsync(new ScanRequest
        {
            TableName = _options.TableName,
            FilterExpression = "#type = :type",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#type"] = "Type"
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":type"] = new AttributeValue { S = "Track" }
            }
        });
        return response.Items.Select(MapTrack).ToList();
    }

    private Track MapTrack(Dictionary<string, AttributeValue> item)
    {
        var track = new Track
        {
            Id = item["Id"].S,
            Title = item["Title"].S,
            Artist = item.ContainsKey("Artist") ? item["Artist"].S : string.Empty,
            Lyrics = item.ContainsKey("Lyrics") ? JsonSerializer.Deserialize<Lyrics>(item["Lyrics"].S) : null
        };
        return track;
    }
}