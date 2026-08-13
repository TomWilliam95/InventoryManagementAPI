using InventoryManagementAPI.Models.CoreModels.UserModels;
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
            await context.Users.AddAsync(testUser, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear(); // Clear the change tracker to ensure we fetch from the database

            var repository = new UserRepository(context);

            //Act
            var result = await repository.GetUserByIdAsync(testUser.ID, CancellationToken.None);

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
            var result = await repository.GetUserByIdAsync(int.MaxValue, CancellationToken.None);

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
            testUser1.Email = $"user1-{Guid.NewGuid():N}@example.com";
            var testUser2 = CreateTestUser();
            testUser2.UserName = "TestUser2";
            testUser2.Email = $"user2-{Guid.NewGuid():N}@example.com";
            await context.Users.AddRangeAsync([testUser1, testUser2], CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);
            //Act
            var result = await repository.GetAllUsersAsync(CancellationToken.None);

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
            await context.AddAsync(testUser, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear(); // Clear the change tracker to ensure we fetch from the database

            var repository = new UserRepository(context);
            //Act
            var result = await repository.GetUserByEmailAsync(testUser.Email, CancellationToken.None);
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
            var result = await repository.GetUserByEmailAsync("nonexistent@example.com", CancellationToken.None);
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

            var roleName = $"Admin-{Guid.NewGuid():N}";
            var adminRole = new Role { Name = roleName };
            var testUser1 = CreateTestUser();
            testUser1.UserName = "AdminUser1";
            testUser1.Email = $"admin1-{Guid.NewGuid():N}@example.com";
            testUser1.UserRoles.Add(new UserRole { Role = adminRole, Created = DateTime.UtcNow });
            var testUser2 = CreateTestUser();
            testUser2.UserName = "AdminUser2";
            testUser2.Email = $"admin2-{Guid.NewGuid():N}@example.com";
            testUser2.UserRoles.Add(new UserRole { Role = adminRole, Created = DateTime.UtcNow });

            await context.Users.AddRangeAsync([testUser1, testUser2], CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.GetUsersByRoleAsync(roleName, CancellationToken.None);

            //Assert
            Assert.NotEmpty(result);
            Assert.Contains(result, u => u.UserName == "AdminUser1");
            Assert.Contains(result, u => u.UserName == "AdminUser2");
        }

        [Fact]
        public async Task GetUsersByRoleAsync_MixedRoles_ReturnsOnlyMatchingUsers()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var adminRoleName = $"Admin-{Guid.NewGuid():N}";
            var adminRole = new Role { Name = adminRoleName };
            var staffRole = new Role { Name = $"Staff-{Guid.NewGuid():N}" };
            var testUser1 = CreateTestUser();
            testUser1.UserName = "AdminUser1";
            testUser1.Email = $"admin-{Guid.NewGuid():N}@example.com";
            testUser1.UserRoles.Add(new UserRole { Role = adminRole, Created = DateTime.UtcNow });
            var testUser2 = CreateTestUser();
            testUser2.UserName = "StaffUser1";
            testUser2.Email = $"staff-{Guid.NewGuid():N}@example.com";
            testUser2.UserRoles.Add(new UserRole { Role = staffRole, Created = DateTime.UtcNow });

            await context.Users.AddRangeAsync([testUser1, testUser2], CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.GetUsersByRoleAsync(adminRoleName, CancellationToken.None);

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
            await context.Users.AddAsync(testUser, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.UserExistsAsync(testUser.ID, CancellationToken.None);

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
            var result = await repository.UserExistsAsync(int.MaxValue, CancellationToken.None);

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
            await context.Users.AddAsync(testUser, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.EmailExistsAsync("testEmail@example.com", CancellationToken.None);

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
            var result = await repository.EmailExistsAsync("nonExisting@email.com", CancellationToken.None);

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
            await context.Users.AddAsync(testUser, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.IsUserActiveAsync(testUser.ID, CancellationToken.None);

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
            await context.Users.AddAsync(testUser, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var repository = new UserRepository(context);

            //Act
            var result = await repository.IsUserActiveAsync(testUser.ID, CancellationToken.None);

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
            var result = await repository.CreateUserAsync(testUser, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            var userId = result.ID;
            context.ChangeTracker.Clear();

            //Assert
            var persistedUser = await context.Users.FindAsync([userId], CancellationToken.None);
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
