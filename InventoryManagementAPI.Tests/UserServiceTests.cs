using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.UserRepositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace InventoryManagementAPI.Tests;

// These tests exercise the real UserService while replacing IUserRepository
// with a Moq-generated repository. This isolates business rules such as
// validation, ownership, password hashing, roles, and active status from SQL.
//
// Setup/ReturnsAsync controls what the fake repository returns.
// Assert checks the service response.
// Verify checks the interaction between the service and repository.
public class UserServiceTests
{
    [Fact]
    public async Task GetUserById_ExistingUser_Returns200()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(CreateUser());
        var service = new UserService(repository.Object);

        // Act
        var result = await service.GetUserByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Successfully retrieved user!", result.Message);
        Assert.Equal("test@example.com", result.Data!.Email);

        // Verify
        repository.Verify(repo => repo.GetUserByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetUserById_MissingUser_Returns404()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.GetUserByIdAsync(99)).ReturnsAsync((User?)null);
        var service = new UserService(repository.Object);

        // Act
        var result = await service.GetUserByIdAsync(99);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("User not found", result.Message);

        // Verify
        repository.Verify(repo => repo.GetUserByIdAsync(99), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmail_ExistingUser_Returns200()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.GetUserByEmailAsync("test@example.com")).ReturnsAsync(CreateUser());
        var service = new UserService(repository.Object);

        // Act
        var result = await service.GetUserByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Successfully retrieved user!", result.Message);

        // Verify
        repository.Verify(repo => repo.GetUserByEmailAsync("test@example.com"), Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_NoUsers_Returns404()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.GetAllUsersAsync()).ReturnsAsync([]);
        var service = new UserService(repository.Object);

        // Act
        var result = await service.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("No User's found", result.Message);

        // Verify
        repository.Verify(repo => repo.GetAllUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUsersByRole_ValidRole_ReturnsMatchingUsers()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.GetUsersByRoleAsync(UserRoles.Staff))
            .ReturnsAsync([CreateUser()]);
        var service = new UserService(repository.Object);

        // Act
        var result = await service.GetUsersByRoleAsync(UserRoles.Staff);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Successfully retrieved list of users!", result.Message);
        Assert.Single(result.Data!);

        // Verify
        repository.Verify(repo => repo.GetUsersByRoleAsync(UserRoles.Staff), Times.Once);
    }

    [Fact]
    public async Task GetUsersByRole_InvalidRole_Returns400()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        var service = new UserService(repository.Object);

        // Act
        var result = await service.GetUsersByRoleAsync((UserRoles)999);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Invalid User Role", result.Message);

        // Verify
        repository.Verify(repo => repo.GetUsersByRoleAsync(It.IsAny<UserRoles>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_ValidRequest_HashesPasswordAndReturns201()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();

        // UserService creates the User entity internally. This variable captures
        // that entity when the service passes it to the mocked repository.
        User? savedUser = null;

        // It.IsAny<User>() means this setup matches any User argument.
        // Callback receives the actual argument and stores it for later assertions.
        // ReturnsAsync returns that same argument, simulating a repository that
        // saves and returns the newly created entity.
        repository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
            .Callback<User>(user => savedUser = user)
            .ReturnsAsync((User user) => user);

        // repository.Object is the generated IUserRepository implementation that
        // can be injected into the real UserService.
        var service = new UserService(repository.Object);

        // Act
        var result = await service.CreateUserAsync(CreateUserRequest());

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal("User created successfully", result.Message);
        Assert.NotNull(savedUser);

        // Ensure the service never sent the plaintext password to persistence.
        Assert.NotEqual("Password1!", savedUser!.Password_Hash);

        // This proves the captured value is a valid hash of the submitted password.
        Assert.True(BCrypt.Net.BCrypt.EnhancedVerify("Password1!", savedUser.Password_Hash));

        // New accounts must always begin with the Staff role.
        Assert.Equal(UserRoles.Staff, savedUser.Role);

        // Verify that the service attempted exactly one insert.
        repository.Verify(repo => repo.CreateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_Returns400()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.EmailExistsAsync("test@example.com")).ReturnsAsync(true);
        var service = new UserService(repository.Object);

        // Act
        var result = await service.CreateUserAsync(CreateUserRequest());

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Email is already in use.", result.Message);

        // Verify
        repository.Verify(repo => repo.CreateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_WeakPassword_Returns400()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        var request = CreateUserRequest();
        request.Password = "weak";
        request.RetypePassword = "weak";
        var service = new UserService(repository.Object);

        // Act
        var result = await service.CreateUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Password must be at least 8 characters long and include uppercase, lowercase, digit, and special character.", result.Message);

        // Verify
        repository.Verify(repo => repo.CreateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEmail_OwnerWithUniqueEmail_Returns200()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(CreateUser());
        var service = new UserService(repository.Object);

        // Act
        var result = await service.UpdateUserEmailAsync(
            1, new UpdateUserEmailRequestDTO { Email = "new@example.com", RowVersion = CreateRowVersion() }, 1, "Staff");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Email updated successfully", result.Message);
        Assert.Equal("new@example.com", result.Data!.Email);

        // Verify
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateEmail_DifferentOrdinaryUser_Returns403()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        var service = new UserService(repository.Object);

        // Act
        var result = await service.UpdateUserEmailAsync(
            1, new UpdateUserEmailRequestDTO { Email = "new@example.com", RowVersion = CreateRowVersion() }, 2, "Staff");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal("Unauthorized! Can only update your own account.", result.Message);

        // Verify
        repository.Verify(repo => repo.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEmail_DuplicateEmail_Returns400()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(CreateUser());
        repository.Setup(repo => repo.EmailExistsAsync("used@example.com")).ReturnsAsync(true);
        var service = new UserService(repository.Object);

        // Act
        var result = await service.UpdateUserEmailAsync(
            1, new UpdateUserEmailRequestDTO { Email = "used@example.com", RowVersion = CreateRowVersion() }, 1, "Staff");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Email is already in use.", result.Message);

        // Verify
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateUsername_InvalidName_Returns400()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        var service = new UserService(repository.Object);

        // Act
        var result = await service.UpdateUserNameAsync(
            1, new UpdateUserNameRequestDTO { UserName = "bad name", RowVersion = CreateRowVersion() }, 1, "Staff");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Invalid UserName", result.Message);

        // Verify
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdatePassword_CorrectCurrentPassword_RehashesAndReturns200()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        var user = CreateUser();
        repository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(user);
        var service = new UserService(repository.Object);

        // Act
        var result = await service.UpdateUserPasswordAsync(1, new UpdateUserPasswordRequestDTO
        {
            CurrentPassword = "Password1!",
            NewPassword = "NewPassword2!",
            RetypePassword = "NewPassword2!",
            RowVersion = CreateRowVersion()
        }, 1, "Staff");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Password updated successfully", result.Message);
        Assert.True(BCrypt.Net.BCrypt.EnhancedVerify("NewPassword2!", user.Password_Hash));

        // Verify
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdatePassword_IncorrectCurrentPassword_Returns400()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(CreateUser());
        var service = new UserService(repository.Object);

        // Act
        var result = await service.UpdateUserPasswordAsync(1, new UpdateUserPasswordRequestDTO
        {
            CurrentPassword = "WrongPassword1!",
            NewPassword = "NewPassword2!",
            RetypePassword = "NewPassword2!",
            RowVersion = CreateRowVersion()
        }, 1, "Staff");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Current password is incorrect", result.Message);

        // Verify
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateRole_ValidRole_Returns200()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(CreateUser());
        var service = new UserService(repository.Object);

        // Act
        var result = await service.UpdateUserRoleAsync(
            1, new UpdateUserRoleRequestDTO { NewRole = UserRoles.Manager, RowVersion = CreateRowVersion() });

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("User role updated successfully", result.Message);
        Assert.Equal(UserRoles.Manager, result.Data!.Role);

        // Verify
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ActivateUser_InactiveUser_Returns200()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        var user = CreateUser();
        user.IsActive = false;
        repository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(user);
        var service = new UserService(repository.Object);

        // Act
        var result = await service.ActivateUserAsync(1, new UpdateUserStatusRequestDTO { IsActive = true, RowVersion = CreateRowVersion() });

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("User activated successfully", result.Message);
        Assert.True(result.Data!.IsActive);

        // Verify
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ActivateUser_AlreadyActive_Returns400()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(CreateUser());
        var service = new UserService(repository.Object);

        // Act
        var result = await service.ActivateUserAsync(1, new UpdateUserStatusRequestDTO { IsActive = true, RowVersion = CreateRowVersion() });

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("User is already active", result.Message);

        // Verify
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeactivateUser_ActiveUser_Returns200()
    {
        // Arrange
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(CreateUser());
        var service = new UserService(repository.Object);

        // Act
        var result = await service.DeactivateUserAsync(1, new UpdateUserStatusRequestDTO { IsActive = false, RowVersion = CreateRowVersion() });

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("User deactivated successfully", result.Message);
        Assert.False(result.Data!.IsActive);

        // Verify
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserEmail_MissingRowVersion_Returns400()
    {
        var repository = new Mock<IUserRepository>();
        var service = new UserService(repository.Object);

        var result = await service.UpdateUserEmailAsync(
            1,
            new UpdateUserEmailRequestDTO { Email = "new@example.com" },
            1,
            "Staff");

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        repository.Verify(repo => repo.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateUserEmail_InvalidRowVersion_Returns400()
    {
        var repository = new Mock<IUserRepository>();
        var service = new UserService(repository.Object);

        var result = await service.UpdateUserEmailAsync(
            1,
            new UpdateUserEmailRequestDTO { Email = "new@example.com", RowVersion = [1, 2, 3, 4] },
            1,
            "Staff");

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        repository.Verify(repo => repo.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateUserEmail_StaleRowVersion_Returns409()
    {
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(CreateUser());
        var service = new UserService(repository.Object);

        var result = await service.UpdateUserEmailAsync(
            1,
            new UpdateUserEmailRequestDTO { Email = "new@example.com", RowVersion = [8, 7, 6, 5, 4, 3, 2, 1] },
            1,
            "Staff");

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateUserEmail_DatabaseConcurrencyException_Returns409()
    {
        var repository = new Mock<IUserRepository>();
        repository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(CreateUser());
        repository.Setup(repo => repo.SaveChangesAsync()).ThrowsAsync(new DbUpdateConcurrencyException());
        var service = new UserService(repository.Object);

        var result = await service.UpdateUserEmailAsync(
            1,
            new UpdateUserEmailRequestDTO { Email = "new@example.com", RowVersion = CreateRowVersion() },
            1,
            "Staff");

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserEmail_Success_ReturnsUpdatedRowVersion()
    {
        var repository = new Mock<IUserRepository>();
        var user = CreateUser();
        byte[] updatedRowVersion = [9, 10, 11, 12, 13, 14, 15, 16];
        repository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(user);
        repository.Setup(repo => repo.SaveChangesAsync())
            .Callback(() => user.RowVersion = updatedRowVersion)
            .Returns(Task.CompletedTask);
        var service = new UserService(repository.Object);

        var result = await service.UpdateUserEmailAsync(
            1,
            new UpdateUserEmailRequestDTO { Email = "new@example.com", RowVersion = CreateRowVersion() },
            1,
            "Staff");

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(updatedRowVersion, result.Data!.RowVersion);
        repository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    private static User CreateUser()
    {
        // Valid baseline entity for tests that need an existing user.
        // Individual tests change only the property relevant to their scenario.
        return new User
        {
            ID = 1,
            UserName = "TestUser",
            Email = "test@example.com",
            Password_Hash = BCrypt.Net.BCrypt.EnhancedHashPassword("Password1!"),
            Role = UserRoles.Staff,
            IsActive = true,
            RowVersion = CreateRowVersion()
        };
    }

    private static CreateNewUserRequestDTO CreateUserRequest()
    {
        // Valid baseline request. Validation tests start here and deliberately
        // replace one field, keeping the cause of failure unambiguous.
        return new CreateNewUserRequestDTO
        {
            UserName = "TestUser",
            Email = "test@example.com",
            Password = "Password1!",
            RetypePassword = "Password1!"
        };
    }

    private static byte[] CreateRowVersion() => [1, 2, 3, 4, 5, 6, 7, 8];
}
