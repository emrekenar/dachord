namespace Application.Services;

using Domain.Interfaces;
using Domain.Wrappers;
using Domain.Errors;
using Application.Interfaces;
using Application.Requests;
using Domain.Models.User;
using Microsoft.Extensions.Logging;

public class RegisterService(IUserRepository userRepository, ILogger<RegisterService> logger) : IRegisterService
{
    public async Task<Result<User>> ExecuteAsync(RegisterRequest request)
    {
        logger.LogInformation("RegisterService.ExecuteAsync called for email: {Email}", request.Email);
        var existingUser = await userRepository.GetByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            logger.LogWarning("User already exists for email: {Email}", request.Email);
            return Result<User>.Failure(new Error(ErrorCode.UserAlreadyExists, "User with this email already exists."));
        }

        var passwordHash = SecurityService.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            PasswordHash = passwordHash
        };
        await userRepository.CreateUserAsync(user);

        logger.LogInformation("User registered: {Email}", user.Email);
        return Result<User>.Success(user);
    }
}