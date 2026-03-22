using Xunit;
using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Options;

using Domain.Interfaces;
using Domain.Models;
using Domain.Errors;
using Application.Services;
using Application.Requests;
using Application.Configuration;
using Application.Interfaces;

namespace UnitTests.Application.Services;

public class LoginServiceTests
{
    private readonly ILoginService _sut;
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
            Id = "existing-user-id",
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
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(ErrorCode.InvalidCredentials);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUnauthorized_WhenPasswordIsIncorrect()
    {
        // Arrange
        var request = new LoginRequest { Email = "existing@example.com", Password = "wrong-password" };

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(ErrorCode.InvalidCredentials);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnOkWithToken_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequest { Email = "existing@example.com", Password = "correct-password" };

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().NotBeEmpty();
    }
}
