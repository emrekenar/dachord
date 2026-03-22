namespace Application.Services;

using Domain.Interfaces;
using Domain.Models;
using Domain.Wrappers;
using Domain.Errors;
using Application.Interfaces;
using Application.Requests;

public class RegisterService(IUserRepository userRepository) : IRegisterService
{
    public async Task<Result<User>> ExecuteAsync(RegisterRequest request)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Result<User>.Failure(new Error(ErrorCode.UserAlreadyExists, "User with this email already exists."));
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            PasswordHash = passwordHash
        };
        await userRepository.CreateUserAsync(user);
        return Result<User>.Success(user);
    }
}