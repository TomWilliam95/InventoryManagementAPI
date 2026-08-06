using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.UserRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Tests.RepositoryTests
{
    public class UserRepositoryTests: IClassFixture<SqlServerFixture>
    {
        private readonly SqlServerFixture _fixture;
        public UserRepositoryTests(SqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        //GetUserByID
        [Fact]
        public async Task GetUserByIdAsync_ExistingUser_ReturnsUser()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testUser = CreateTestUser();
            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear(); // Clear the change tracker to ensure we fetch from the database

            var repository = new UserRepository(context);

            //Act
            var result = await repository.GetUserByIdAsync(testUser.ID);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testUser.ID, result.ID);
        }
        [Fact]
        public async Task GetUserByIdAsync_NoMatchingId_ReturnsNull()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            var repository = new UserRepository(context);

            //Act
            var result = await repository.GetUserByIdAsync(int.MaxValue);

            //Assert
            Assert.Null(result);
        }

        //GetAllUsers
        [Fact]
        public async Task GetAllUsersAsync_UsersExist_ReturnsUsers()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testUser1 = CreateTestUser();
            testUser1.UserName = "TestUser1";
            var testUser2 = CreateTestUser();
            testUser2.UserName = "TestUser2";
            await context.Users.AddRangeAsync(testUser1, testUser2);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);
            //Act
            var result = await repository.GetAllUsersAsync();

            //Assert
            Assert.NotEmpty(result);
            Assert.Contains(result, u => u.UserName == "TestUser1");
            Assert.Contains(result, u => u.UserName == "TestUser2");
        }

        //GetUserByEmail
        [Fact]
        public async Task GetUserByEmailAsync_ExistingUser_ReturnsUser()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testUser = CreateTestUser();
            testUser.Email = "testEmail@example.com";
            await context.AddAsync(testUser);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear(); // Clear the change tracker to ensure we fetch from the database

            var repository = new UserRepository(context);
            //Act
            var result = await repository.GetUserByEmailAsync(testUser.Email);
            //Assert
            Assert.NotNull(result);
            Assert.Equal(testUser.Email, result.Email);
        }
        [Fact]
        public async Task GetUserByEmailAsync_NoMatchingEmail_ReturnsNull()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var repository = new UserRepository(context);
            //Act
            var result = await repository.GetUserByEmailAsync("nonexistent@example.com");
            //Assert
            Assert.Null(result);
        }

        //GetUserByRole
        [Fact]
        public async Task GetUsersByRoleAsync_MatchingUsers_ReturnsUsers()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testUser1 = CreateTestUser();
            testUser1.UserName = "AdminUser1";
            testUser1.Role = UserRoles.Admin;
            var testUser2 = CreateTestUser();
            testUser2.Role = UserRoles.Admin;
            testUser2.UserName = "AdminUser2";

            await context.Users.AddRangeAsync(testUser1, testUser2);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.GetUsersByRoleAsync(UserRoles.Admin);

            //Assert
            Assert.NotEmpty(result);
            Assert.Contains(result, u => u.UserName == "AdminUser1");
            Assert.Contains(result, u => u.UserName == "AdminUser2");
            Assert.All(result, u => Assert.Equal(UserRoles.Admin, u.Role));
        }

        [Fact]
        public async Task GetUsersByRoleAsync_MixedRoles_ReturnsOnlyMatchingUsers()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testUser1 = CreateTestUser();
            testUser1.UserName = "AdminUser1";
            testUser1.Role = UserRoles.Admin;
            var testUser2 = CreateTestUser();
            testUser2.Role = UserRoles.Staff;
            testUser2.UserName = "StaffUser1";

            await context.Users.AddRangeAsync(testUser1, testUser2);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.GetUsersByRoleAsync(UserRoles.Admin);

            //Assert
            Assert.NotEmpty(result);
            Assert.Contains(result, u => u.UserName == "AdminUser1");
            Assert.DoesNotContain(result, u => u.UserName == "StaffUser1");
        }

        //CheckUserExists
        [Fact]
        public async Task UserExistsAsync_CheckMatchingUserId_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testUser = CreateTestUser();
            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.UserExistsAsync(testUser.ID);

            //Assert
            Assert.True(result);
        }
        [Fact]
        public async Task UserExistsAsync_CheckNonMatchingUserId_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.UserExistsAsync(int.MaxValue);

            //Assert
            Assert.False(result);
        }

        //CheckEmailExists
        [Fact]
        public async Task EmailExistsAsync_CheckMatchingEmail_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testUser = CreateTestUser();
            testUser.Email = "testEmail@example.com";
            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.EmailExistsAsync("testEmail@example.com");

            //Assert
            Assert.True(result);
        }
        [Fact]
        public async Task EmailExistsAsync_CheckNonMatchingEmail_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.EmailExistsAsync("nonExisting@email.com");

            //Assert
            Assert.False(result);
        }

        //IsUserActive
        [Fact]
        public async Task IsUserActiveAsync_CheckActiveUser_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testUser = CreateTestUser();
            testUser.IsActive = true;
            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.IsUserActiveAsync(testUser.ID);

            //Assert
            Assert.True(result);
        }
        [Fact]
        public async Task IsUserActiveAsync_CheckInactiveUser_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testUser = CreateTestUser();
            testUser.IsActive = false;
            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.IsUserActiveAsync(testUser.ID);

            //Assert
            Assert.False(result);
        }


        //CreateUser
        [Fact]
        public async Task CreateUserAsync_ValidUser_PersistsUser()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            var testUser = CreateTestUser();
            var repository = new UserRepository(context);
            
            //Act
            var result = await repository.CreateUserAsync(testUser);
            await context.SaveChangesAsync();
            var userId = result.ID;
            context.ChangeTracker.Clear();

            //Assert
            var persistedUser = await context.Users.FindAsync(userId);
            Assert.NotNull(persistedUser);
            Assert.Equal(testUser.UserName, persistedUser.UserName);
            Assert.Equal(testUser.Email, persistedUser.Email);
        }

        private static User CreateTestUser()
        {
            return new User
            {
                UserName = "Test",
                Password_Hash = "Password",
                Email = "test.user@example.com"
            };
        }
    }
}
