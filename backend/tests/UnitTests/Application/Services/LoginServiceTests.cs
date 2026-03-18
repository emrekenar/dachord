using Xunit;
using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.HttpResults;

using Application.Services;
using Application.Requests;
using Application.Responses;
using Application.Configuration;
using Domain.Interfaces;
using Domain.Models;

namespace UnitTests.Application.Services;

public class LoginServiceTests
{
    private readonly LoginService _sut;
    public LoginServiceTests()
    {
        var jwtOptions = Substitute.For<IOptions<JwtOptions>>();
        jwtOptions.Value.Returns(new JwtOptions {
            Key = "super_secret_test_key_at_least_16_chars", 
            Issuer = "issuer",
            Audience = "audience",
            ExpireMinutes = 60
        });

        var userRepo = Substitute.For<IUserRepository>();
        var user = new User
        {
            Id = 100,
            Email = "existing@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password"),
        };
        userRepo.GetByEmailAsync("existing@example.com").Returns(user);

        _sut = new LoginService(jwtOptions, userRepo);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenUserNotFound()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "password" };

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenPasswordIsIncorrect()
    {
        // Arrange
        var request = new LoginRequest { Email = "existing@example.com", Password = "wrong-password" };

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnOkWithToken_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequest { Email = "existing@example.com", Password = "correct-password" };

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<Ok<LoginResponse>>().Subject;
        okResult.Value.Should().NotBeNull();
        okResult.Value!.Token.Should().NotBeEmpty();
    }
}
