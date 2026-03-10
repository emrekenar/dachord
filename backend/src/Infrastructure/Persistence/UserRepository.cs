namespace Infrastructure.Persistence;

using Domain.Interfaces;
using Domain.Models;

public class UserRepository : IUserRepository
{
    private readonly Dictionary<int, User> _users = new Dictionary<int, User>();

    private int _nextId = 0;

    public async Task<User?> GetByIdAsync(int id)
    {
        _users.TryGetValue(id, out var user);
        return await Task.FromResult(user);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var user = _users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return await Task.FromResult(user);
    }

    public async Task CreateUserAsync(User user)
    {
        user.Id = _nextId++;
        _users[user.Id] = user;
        await Task.CompletedTask;
    }
}