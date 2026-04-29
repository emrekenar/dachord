namespace Infrastructure.Persistence;

using Amazon.DynamoDBv2.DataModel;

using Domain.Interfaces;
using Domain.Models.User;
using Infrastructure.Entities;
using Infrastructure.Mappers;

public class UserRepository(IDynamoDBContext dynamoDbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(string id)
    {
        var item = await dynamoDbContext.LoadAsync<UserItem>(id);
        return item is null ? null : UserMapper.MapToDomainModel(item);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var conditions = new List<ScanCondition>
        {
            new("Email", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, email)
        };
        var search = dynamoDbContext.ScanAsync<UserItem>(conditions);
        var items = await search.GetRemainingAsync();
        var item = items.FirstOrDefault();
        return item is null ? null : UserMapper.MapToDomainModel(item);
    }

    public async Task CreateUserAsync(User user)
    {
        var item = UserMapper.MapToEntity(user);
        await dynamoDbContext.SaveAsync(item);
    }
}
