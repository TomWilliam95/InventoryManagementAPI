using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.AuthenticationRepositories;
using InventoryManagementAPI.Repositories.JWT;
using InventoryManagementAPI.Repositories.UserRepositories;
using Moq;

namespace InventoryManagementAPI.Tests;

// These are unit tests for AuthService. The real user database and JWT generator
// are replaced with mocks so each test controls one login scenario.
//
// Every test follows Arrange, Act, Assert, Verify:
// Arrange = prepare inputs and mock behaviour.
// Act     = call the real service method.
// Assert  = inspect the ApiResponse returned by the service.
// Verify  = confirm the service used (or did not use) its dependencies correctly.
public class AuthServiceTests
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var tokenService = new Mock<IJwtTokenService>();
        var user = CreateUser();

        // Simulate a successful database lookup for the submitted email.
        userRepository.Setup(repo => repo.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);

        // A predictable fake token lets the test check the response without
        // generating or signing a real JWT.
        tokenService.Setup(service => service.GenerateToken(user)).Returns("test-token");

        // .Object exposes implementations of the interfaces expected by AuthService.
        var service = new AuthService(userRepository.Object, tokenService.Object);

        // Act
        var result = await service.LoginAsync(CreateLoginRequest());

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("User authenticated successfully", result.Message);
        Assert.Equal("test-token", result.Data!.Token);

        // Successful authentication must save LastLogin and issue exactly one token.
        userRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        tokenService.Verify(jwt => jwt.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task Login_MissingCredentials_Returns400()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var tokenService = new Mock<IJwtTokenService>();
        var service = new AuthService(userRepository.Object, tokenService.Object);

        // Act
        var result = await service.LoginAsync(null!);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Email and Password are required", result.Message);

        // Verify
        tokenService.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var tokenService = new Mock<IJwtTokenService>();

        // Returning a typed null simulates an email lookup with no matching user.
        userRepository.Setup(repo => repo.GetUserByEmailAsync("test@example.com"))
            .ReturnsAsync((User?)null);
        var service = new AuthService(userRepository.Object, tokenService.Object);

        // Act
        var result = await service.LoginAsync(CreateLoginRequest());

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(401, result.StatusCode);
        Assert.Equal("Invalid Email or Password", result.Message);
        tokenService.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var tokenService = new Mock<IJwtTokenService>();
        userRepository.Setup(repo => repo.GetUserByEmailAsync("test@example.com"))
            .ReturnsAsync(CreateUser());
        var service = new AuthService(userRepository.Object, tokenService.Object);
        var request = CreateLoginRequest();
        request.Password = "WrongPassword1!";

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(401, result.StatusCode);
        Assert.Equal("Invalid Email or Password", result.Message);

        // Verify
        tokenService.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_InactiveUser_Returns401()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var tokenService = new Mock<IJwtTokenService>();
        var user = CreateUser();
        user.IsActive = false;
        userRepository.Setup(repo => repo.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
        var service = new AuthService(userRepository.Object, tokenService.Object);

        // Act
        var result = await service.LoginAsync(CreateLoginRequest());

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(401, result.StatusCode);
        Assert.Equal("Invalid Email or Password", result.Message);

        // Verify
        tokenService.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_RepositoryThrows_Returns500()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var tokenService = new Mock<IJwtTokenService>();
        userRepository.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));
        var service = new AuthService(userRepository.Object, tokenService.Object);

        // Act
        var result = await service.LoginAsync(CreateLoginRequest());

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal("Internal error occurred, failed to authenticate user.", result.Message);

        // Verify
        tokenService.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    private static User CreateUser()
    {
        // Use a real BCrypt hash because AuthService performs real password checking.
        return new User
        {
            ID = 1,
            UserName = "TestUser",
            Email = "test@example.com",
            Password_Hash = BCrypt.Net.BCrypt.EnhancedHashPassword("Password1!"),
            Role = UserRoles.Staff,
            IsActive = true
        };
    }

    private static LoginRequestDTO CreateLoginRequest()
    {
        // Valid login input used as the default for each scenario.
        return new LoginRequestDTO
        {
            Email = "test@example.com",
            Password = "Password1!"
        };
    }
}
