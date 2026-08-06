using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.InvMovementRepositories;
using InventoryManagementAPI.Services;

namespace InventoryManagementAPI.Tests.RepositoryTests
{
    public class MovementRepositoryTests : IClassFixture<SqlServerFixture>
    {
        private readonly SqlServerFixture _fixture;
        public MovementRepositoryTests(SqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        //GetMovementById
        [Fact]
        public async Task GetMovementByIdAsync_ExistingMovement_ReturnsMovement()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductSupplierCategory(context);
            var testUser = await CreateTestUser(context);

            var testMovement = CreateMovement(MovementType.Sale, 10, "Test movement", testProduct.ID, testUser.ID);
            await context.InventoryMovements.AddAsync(testMovement);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear(); // Clear the change tracker to ensure we fetch from the database

            var movementRepository = new InventoryMovementRepository(context);

            // Act
            var movement = await movementRepository.GetMovementByIdAsync(testMovement.ID);

            // Assert
            Assert.NotNull(movement);
            Assert.Equal(testMovement.ID, movement.ID);
            Assert.Equal("Test movement", movement.Reason);
        }
        [Fact]
        public async Task GetMovementByIdAsync_MissingMovement_ReturnsNull()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            var movementRepository = new InventoryMovementRepository(context);

            //Act
            var movement = await movementRepository.GetMovementByIdAsync(int.MaxValue);

            //Assert
            Assert.Null(movement);
        }

        //GetAllMovements
        [Fact]
        public async Task GetAllMovementsAsync_MovementsExist_ReturnsMovements()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductSupplierCategory(context);
            var testUser = await CreateTestUser(context);

            var testMovement1 = CreateMovement(MovementType.Sale, 10, "Test movement 1", testProduct.ID, testUser.ID);
            var testMovement2 = CreateMovement(MovementType.Purchase, 20, "Test movement 2", testProduct.ID, testUser.ID);
            var testMovement3 = CreateMovement(MovementType.StockIn, 30, "Test movement 3", testProduct.ID, testUser.ID);
            await context.InventoryMovements.AddRangeAsync(testMovement1, testMovement2, testMovement3);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var movementRepository = new InventoryMovementRepository(context);

            //Act
            var movements = await movementRepository.GetAllMovementsAsync();

            //Assert
            Assert.NotNull(movements);
            Assert.Contains(movements, m => m.Reason == "Test movement 1");
            Assert.Contains(movements, m => m.Reason == "Test movement 2");
            Assert.Contains(movements, m => m.Reason == "Test movement 3");
        }

        //GetMovementsByProductId
        [Fact]
        public async Task GetMovementsByProductIdAsync_MatchingMovements_ReturnsMovements()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductSupplierCategory(context);
            var testUser = await CreateTestUser(context);

            var testMovement1 = CreateMovement(MovementType.Sale, 10, "Test movement 1", testProduct.ID, testUser.ID);
            var testMovement2 = CreateMovement(MovementType.Purchase, 20, "Test movement 2", testProduct.ID, testUser.ID);
            var testMovement3 = CreateMovement(MovementType.StockIn, 30, "Test movement 3", testProduct.ID, testUser.ID);
            await context.InventoryMovements.AddRangeAsync(testMovement1, testMovement2, testMovement3);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var movementRepository = new InventoryMovementRepository(context);

            //Act
            var movements = await movementRepository.GetMovementsByProductIdAsync(testProduct.ID);

            //Assert
            Assert.NotNull(movements);
            Assert.Contains(movements, m => m.Reason == "Test movement 1");
            Assert.Contains(movements, m => m.Reason == "Test movement 2");
            Assert.Contains(movements, m => m.Reason == "Test movement 3");
        }
        [Fact]
        public async Task GetMovementsByProductIdAsync_MissingProduct_ReturnsEmpty()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var existingProduct = await CreateTestProductSupplierCategory(context);
            var testUser = await CreateTestUser(context);
            var existingMovement = CreateMovement(
                MovementType.StockIn,
                5,
                "Movement for another product",
                existingProduct.ID,
                testUser.ID);
            await context.InventoryMovements.AddAsync(existingMovement);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var movementRepository = new InventoryMovementRepository(context);

            //Act
            var movements = await movementRepository.GetMovementsByProductIdAsync(int.MaxValue);

            //Assert
            Assert.NotNull(movements);
            Assert.Empty(movements);
        }
        [Fact]
        public async Task GetMovementsByProductIdAsync_ProductHasNoMovements_ReturnsEmpty()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            //Creates Test Product,Supplier,Category || Returns Product
            var testProduct = await CreateTestProductSupplierCategory(context);

            var movementRepository = new InventoryMovementRepository(context);

            //Act
            var result = await movementRepository.GetMovementsByProductIdAsync(testProduct.ID);

            //Assert
            Assert.NotNull(await context.Products.FindAsync(testProduct.ID));
            Assert.Empty(result);
        }

        //GetMovementsByUserId
        [Fact]
        public async Task GetMovementsByUserIdAsync_MatchingMovements_ReturnsMovements()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductSupplierCategory(context);
            var testUser = await CreateTestUser(context);

            var testMovement1 = CreateMovement(MovementType.Sale, 10, "Test movement 1", testProduct.ID, testUser.ID);
            var testMovement2 = CreateMovement(MovementType.Purchase, 20, "Test movement 2", testProduct.ID, testUser.ID);
            var testMovement3 = CreateMovement(MovementType.StockIn, 30, "Test movement 3", testProduct.ID, testUser.ID);

            await context.InventoryMovements.AddRangeAsync(testMovement1, testMovement2, testMovement3);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var movementRepository = new InventoryMovementRepository(context);

            //Act
            var movements = await movementRepository.GetMovementsByUserIdAsync(testUser.ID);

            //Assert
            Assert.NotNull(await context.Users.FindAsync(testUser.ID));
            Assert.NotEmpty(movements);
            Assert.Contains(movements, m => m.Reason == "Test movement 1");
            Assert.Contains(movements, m => m.Reason == "Test movement 2");
            Assert.Contains(movements, m => m.Reason == "Test movement 3");
        }
        [Fact]
        public async Task GetMovementsByUserIdAsync_MissingUser_ReturnsEmpty()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductSupplierCategory(context);
            var existingUser = await CreateTestUser(context);
            var existingMovement = CreateMovement(
                MovementType.StockIn,
                5,
                "Movement for another user",
                testProduct.ID,
                existingUser.ID);
            await context.InventoryMovements.AddAsync(existingMovement);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var movementRepository = new InventoryMovementRepository(context);

            //Act
            var result = await movementRepository.GetMovementsByUserIdAsync(int.MaxValue);

            //Assert
            Assert.Null(await context.Users.FindAsync(int.MaxValue));
            Assert.Empty(result);
        }
        [Fact]
        public async Task GetMovementsByUserIdAsync_UserHasNoMovements_ReturnsEmpty()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testUser = await CreateTestUser(context);
            var movementRepository = new InventoryMovementRepository(context);

            //Act
            var result = await movementRepository.GetMovementsByUserIdAsync(testUser.ID);

            //Assert
            Assert.NotNull(await context.Users.FindAsync(testUser.ID));
            Assert.Empty(result);
        }

        //GetMovementsByType
        [Fact]
        public async Task GetMovementsByTypeAsync_MatchingMovements_ReturnsMovements()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductSupplierCategory(context);
            var testUser = await CreateTestUser(context);

            var testMovement1 = CreateMovement(MovementType.Sale, 10, "Test movement 1", testProduct.ID, testUser.ID);
            var testMovement2 = CreateMovement(MovementType.Sale, 20, "Test movement 2", testProduct.ID, testUser.ID);
            await context.InventoryMovements.AddRangeAsync(testMovement1, testMovement2);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var movementRepository = new InventoryMovementRepository(context);
            
            //Act
            var movements = await movementRepository.GetMovementsByTypeAsync(MovementType.Sale);

            //Assert
            Assert.NotNull(movements);
            Assert.Contains(movements, m => m.Reason == "Test movement 1");
            Assert.Contains(movements, m => m.Reason == "Test movement 2");
        }
        [Fact]
        public async Task GetMovementsByTypeAsync_NoMatchingMovements_ReturnsEmpty()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductSupplierCategory(context);
            var testUser = await CreateTestUser(context);
            var saleMovement = CreateMovement(
                MovementType.Sale,
                5,
                "Non-matching sale movement",
                testProduct.ID,
                testUser.ID);
            await context.InventoryMovements.AddAsync(saleMovement);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var movementRepository = new InventoryMovementRepository(context);

            //Act
            var result = await movementRepository.GetMovementsByTypeAsync(MovementType.AdjustmentIncrease);

            //Assert
            Assert.Empty(result);
        }

        //GetMovementByDateRange
        [Fact]
        public async Task GetMovementsByDateRangeAsync_MatchingMovements_ReturnsMovements()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductSupplierCategory(context);
            var testUser = await CreateTestUser(context);

            var testMovement1 = CreateMovement(MovementType.Sale, 10, "Test movement 1", testProduct.ID, testUser.ID);
            var testMovement2 = CreateMovement(MovementType.Purchase, 20, "Test movement 2", testProduct.ID, testUser.ID);
            await context.InventoryMovements.AddRangeAsync(testMovement1, testMovement2);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var movementRepository = new InventoryMovementRepository(context);

            //Act
            var startDate = DateTime.UtcNow.AddMinutes(-1);
            var endDate = DateTime.UtcNow.AddMinutes(1);

            var movements = await movementRepository.GetMovementsByDateRangeAsync(startDate, endDate);

            //Assert
            Assert.NotNull(movements);
            Assert.Contains(movements, m => m.Reason == "Test movement 1");
            Assert.Contains(movements, m => m.Reason == "Test movement 2");
        }
        [Fact]
        public async Task GetMovementsByDateRangeAsync_NoMovementsWithinRange_ReturnsEmpty()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductSupplierCategory(context);
            var testUser = await CreateTestUser(context);
            var currentMovement = CreateMovement(
                MovementType.StockIn,
                5,
                "Movement outside requested range",
                testProduct.ID,
                testUser.ID);
            currentMovement.Created = DateTime.UtcNow;
            await context.InventoryMovements.AddAsync(currentMovement);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var movementRepository = new InventoryMovementRepository(context);

            //Act
            var startDate = DateTime.UtcNow.AddDays(-3);
            var endDate = DateTime.UtcNow.AddDays(-2);
            var result = await movementRepository.GetMovementsByDateRangeAsync(startDate, endDate);

            //Assert
            Assert.Empty(result);
        }

        //CreateMovement
        [Fact]
        public async Task AddMovementAsync_ValidMovement_PersistsMovement()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductSupplierCategory(context);
            var testUser = await CreateTestUser(context);
            var testMovement = CreateMovement(MovementType.Sale, 10, "Test movement", testProduct.ID, testUser.ID);

            var movementRepository = new InventoryMovementRepository(context);

            //Act
            var result = await movementRepository.AddMovementAsync(testMovement);
            await context.SaveChangesAsync();
            var movementId = result.ID;
            context.ChangeTracker.Clear();

            //Assert
            var persistedMovement = await context.InventoryMovements.FindAsync(movementId);
            Assert.NotNull(persistedMovement);
            Assert.Equal("Test movement", persistedMovement.Reason);
        }

        // Helper methods
        private static InventoryMovement CreateMovement(MovementType movement, int quantity, string reason, int productId, int userId)
        {
            return new InventoryMovement
            {
                ProductId = productId,
                Quantity = quantity,
                QuantityBefore = 100,
                Movement = movement,
                UserID = userId,
                Reason = reason,
                Created = DateTime.UtcNow
            };
        }

        private static async Task<Product> CreateTestProductSupplierCategory(InvManDBContext context)
        {
            var testCategory = new Category
            {
                Name = "Test Category",
                Description = "Test Description"
            };
            await context.Categories.AddAsync(testCategory);

            var testSupplier = new Supplier
            {
                Name = "Test Supplier",
                ContactName = "Test Contact",
                Address = "123 Test St",
                PhoneContact = "12345678",
                EmailContact = "test@example.com"
            };
            await context.Suppliers.AddAsync(testSupplier);
            await context.SaveChangesAsync();

            var testProduct = new Product
            {
                Sku = "TESTSKU",
                Name = "Test Product",
                Description = "Test Description",
                CategoryID = testCategory.ID,
                SupplierID = testSupplier.ID,
                QuantityInStock = 100,
            };
            await context.Products.AddAsync(testProduct);

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            return testProduct;
        }

        private static async Task<User> CreateTestUser(InvManDBContext context)
        {
            var testUser = new User
            {
                UserName = "TestUser",
                Email = "test@example.com",
                Password_Hash = "hashedpassword",
            };
            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            return testUser;
        }
    }
}
