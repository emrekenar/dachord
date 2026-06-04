namespace Application.Services;

using Domain.Interfaces;
using Application.Interfaces;

public class UpdateDisplayNameService(IUserRepository userRepository) : IUpdateDisplayNameService
{
    public async Task<bool> ExecuteAsync(string userId, string displayName)
    {
        await userRepository.UpdateDisplayNameAsync(userId, displayName);
        return true;
    }
}
