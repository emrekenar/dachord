namespace Application.Services;

using Application.Interfaces;
using Application.Models;
using Domain.Interfaces;
using Domain.Models;

public class RegisterService(IUserRepository userRepository) : IRegisterService
{
    public async Task<IResult> ExecuteAsync(RegisterRequest request)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Results.BadRequest("User already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash
        };
        await userRepository.CreateUserAsync(user);
        return Results.Created($"/users/{user.Id}", user);
    }
}