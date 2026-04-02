namespace Infrastructure.Persistence;

using Microsoft.Extensions.Options;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Configuration;

public class UserRepository : IUserRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbOptions _options;

    public UserRepository(IAmazonDynamoDB dynamoDb, IOptions<DynamoDbOptions> options)
    {
        _dynamoDb = dynamoDb;
        _options = options.Value;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        var response = await _dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = _options.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id },
                ["Type"] = new AttributeValue { S = "User" }
            }
        });
        if (response.Item == null || response.Item.Count == 0)
            return null;
        return MapUser(response.Item);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var response = await _dynamoDb.QueryAsync(new QueryRequest
        {
            TableName = _options.TableName,
            IndexName = "EmailIndex",
            KeyConditionExpression = "Email = :email AND #type = :type",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#type"] = "Type"
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":email"] = new AttributeValue { S = email },
                [":type"] = new AttributeValue { S = "User" }
            }
        });
        var item = response.Items.FirstOrDefault();
        return item != null ? MapUser(item) : null;
    }

    public async Task CreateUserAsync(User user)
    {
        var id = string.IsNullOrEmpty(user.Id) ? Guid.NewGuid().ToString() : user.Id;
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = id },
            ["Type"] = new AttributeValue { S = "User" },
            ["Email"] = new AttributeValue { S = user.Email },
            ["PasswordHash"] = new AttributeValue { S = user.PasswordHash }
        };
        await _dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = _options.TableName,
            Item = item
        });
        user.Id = id;
    }

    private User MapUser(Dictionary<string, AttributeValue> item)
    {
        return new User
        {
            Id = item["Id"].S,
            Email = item["Email"].S,
            PasswordHash = item["PasswordHash"].S
        };
    }
}