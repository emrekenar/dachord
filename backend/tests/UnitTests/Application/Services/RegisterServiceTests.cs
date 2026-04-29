using FluentAssertions;
using NSubstitute;
using Xunit;

using Domain.Errors;
using Domain.Interfaces;
using Application.Interfaces;
using Application.Requests;
using Application.Services;
using Domain.Models.User;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Application.Services;

public class RegisterServiceTests
{
    private readonly IRegisterService _sut;

    public RegisterServiceTests()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var user = new User
        {
            Id = "existing-user-id",
            Email = "existing@example.com",
            PasswordHash = SecurityService.HashPassword("correct-password"),
        };
        userRepo.GetByEmailAsync("existing@example.com").Returns(user);

        var logger = NullLogger<RegisterService>.Instance;

        _sut = new RegisterService(userRepo, logger);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnBadRequest_WhenUserAlreadyExists()
    {
        // Arrange
        var request = new RegisterRequest { Email = "existing@example.com", Password = "password" };

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(ErrorCode.UserAlreadyExists);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateUser_WhenEmailIsUnique()
    {
        // Arrange
        var request = new RegisterRequest { Email = "unique@example.com", Password = "password" };

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Email.Should().Be("unique@example.com");
        result.Value.PasswordHash.Should().NotBeEmpty();
    }
}