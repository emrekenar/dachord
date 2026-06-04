namespace Infrastructure.Persistence;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;

using Domain.Interfaces;
using Domain.Models.User;
using Infrastructure.Configuration;
using Infrastructure.Entities;
using Infrastructure.Mappers;

public class UserRepository(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB dynamoDbClient, IOptions<DynamoDbOptions> dynamoDbOptions) : IUserRepository
{
    private string UsersTable => dynamoDbOptions.Value.TableNamePrefix + "users";

    public async Task<User?> GetByIdAsync(string id)
    {
        var item = await dynamoDbContext.LoadAsync<UserItem>(id);
        return item is null ? null : UserMapper.MapToDomainModel(item);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var response = await dynamoDbClient.QueryAsync(new QueryRequest
        {
            TableName = UsersTable,
            IndexName = "EmailIndex",
            KeyConditionExpression = "Email = :email",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":email"] = new AttributeValue { S = email }
            },
            Limit = 1
        });
        var raw = response.Items.FirstOrDefault();
        if (raw is null) return null;
        var item = Amazon.DynamoDBv2.DocumentModel.Document.FromAttributeMap(raw);
        return UserMapper.MapToDomainModel(dynamoDbContext.FromDocument<UserItem>(item));
    }

    public async Task CreateUserAsync(User user)
    {
        var item = UserMapper.MapToEntity(user);
        await dynamoDbContext.SaveAsync(item);
    }

    public async Task UpdateDisplayNameAsync(string userId, string displayName)
    {
        var item = await dynamoDbContext.LoadAsync<UserItem>(userId);
        if (item is null) return;
        item.DisplayName = displayName;
        await dynamoDbContext.SaveAsync(item);
    }
}
