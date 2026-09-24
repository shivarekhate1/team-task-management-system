using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Exceptions;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Services;
using Xunit;

namespace TaskManagement.Tests;

public class AuthServiceTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _passwordHasher = new PasswordHasher();

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:SecretKey", "TEST_SUPER_SECRET_KEY_FOR_UNIT_TESTS_1234567890!"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpiryInHours", "1"}
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _jwtTokenGenerator = new JwtTokenGenerator(config);
        _authService = new AuthService(_dbContext, _passwordHasher, _jwtTokenGenerator);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var request = new RegisterRequest("Test User", "test@example.com", "Password@123", UserRole.User);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be("test@example.com");
        result.User.Role.Should().Be(UserRole.User);

        var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        userInDb.Should().NotBeNull();
        _passwordHasher.VerifyPassword("Password@123", userInDb!.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ShouldThrowValidationException()
    {
        // Arrange
        var request = new RegisterRequest("Test User", "duplicate@example.com", "Password@123", UserRole.User);
        await _authService.RegisterAsync(request);

        // Act & Assert
        var duplicateRequest = new RegisterRequest("Another User", "duplicate@example.com", "Password@123", UserRole.User);
        await Assert.ThrowsAsync<ValidationException>(() => _authService.RegisterAsync(duplicateRequest));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var regRequest = new RegisterRequest("Login User", "login@example.com", "SecretPass123", UserRole.Manager);
        await _authService.RegisterAsync(regRequest);

        var loginRequest = new LoginRequest("login@example.com", "SecretPass123");

        // Act
        var response = await _authService.LoginAsync(loginRequest);

        // Assert
        response.Should().NotBeNull();
        response.Token.Should().NotBeNullOrEmpty();
        response.User.Role.Should().Be(UserRole.Manager);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var regRequest = new RegisterRequest("Login User", "wrongpass@example.com", "SecretPass123", UserRole.User);
        await _authService.RegisterAsync(regRequest);

        var loginRequest = new LoginRequest("wrongpass@example.com", "WrongPassword!");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.LoginAsync(loginRequest));
    }
}
