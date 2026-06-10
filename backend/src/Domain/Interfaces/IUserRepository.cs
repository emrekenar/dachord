namespace Domain.Interfaces;

using Domain.Models.User;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task CreateUserAsync(User user);
    Task UpdateDisplayNameAsync(string userId, string displayName);
    Task UpdateProfileAsync(string userId, string? bio, string? avatarIcon);
    Task UpdateRoleAsync(string userId, UserRole role);
    Task<IEnumerable<User>> GetAllUsersAsync();
}