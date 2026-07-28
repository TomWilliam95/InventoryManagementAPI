using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.MovementDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.InvMovementRepositories;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using InventoryManagementAPI.Repositories.UserRepositories;
using Moq;

namespace InventoryManagementAPI.Tests;

// InventoryMovementService has three dependencies, so these tests mock all three:
// the movement repository, product repository, and user repository.
// The service itself remains real. This lets each test simulate inventory
// scenarios without updating a database.
//
// A movement-writing test checks both the returned response and side effects:
// the calculated stock quantity, movement insertion, and SaveChanges call.
public class InventoryMovementServiceTests
{
    [Fact]
    public async Task GetMovementById_ExistingMovement_Returns200()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        movementRepo.Setup(repo => repo.GetMovementByIdAsync(1)).ReturnsAsync(CreateMovement());
        var service = CreateService(movementRepo, productRepo, userRepo);

        // Act
        var result = await service.GetMovementByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Movement retrieved successfully.", result.Message);
        Assert.Equal("Test Product", result.Data!.ProductName);

        // Verify
        movementRepo.Verify(repo => repo.GetMovementByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetMovementById_MissingMovement_Returns404()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        movementRepo.Setup(repo => repo.GetMovementByIdAsync(99))
            .ReturnsAsync((InventoryMovement?)null);
        var service = CreateService(movementRepo, productRepo, userRepo);

        // Act
        var result = await service.GetMovementByIdAsync(99);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Movement not found.", result.Message);

        // Verify
        movementRepo.Verify(repo => repo.GetMovementByIdAsync(99), Times.Once);
    }

    [Fact]
    public async Task GetAllMovements_ExistingMovements_Returns200()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        movementRepo.Setup(repo => repo.GetAllMovementsAsync())
            .ReturnsAsync([CreateMovement(), CreateMovement()]);
        var service = CreateService(movementRepo, productRepo, userRepo);

        // Act
        var result = await service.GetAllMovementsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Movements retrieved successfully.", result.Message);
        Assert.Equal(2, result.Data!.Count());

        // Verify
        movementRepo.Verify(repo => repo.GetAllMovementsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllMovements_NoMovements_Returns404()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        movementRepo.Setup(repo => repo.GetAllMovementsAsync()).ReturnsAsync([]);
        var service = CreateService(movementRepo, productRepo, userRepo);

        // Act
        var result = await service.GetAllMovementsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("No movements found.", result.Message);

        // Verify
        movementRepo.Verify(repo => repo.GetAllMovementsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetProductMovementHistory_MissingProduct_Returns404()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        productRepo.Setup(repo => repo.GetProductAsync(99)).ReturnsAsync((Product?)null);
        var service = CreateService(movementRepo, productRepo, userRepo);

        // Act
        var result = await service.GetProductMovementHistoryAsync(99);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Product not found.", result.Message);

        // Verify
        movementRepo.Verify(repo => repo.GetMovementsByProductIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetProductMovementHistory_ExistingHistory_Returns200()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        productRepo.Setup(repo => repo.GetProductAsync(1)).ReturnsAsync(CreateProduct());
        movementRepo.Setup(repo => repo.GetMovementsByProductIdAsync(1))
            .ReturnsAsync([CreateMovement()]);
        var service = CreateService(movementRepo, productRepo, userRepo);

        // Act
        var result = await service.GetProductMovementHistoryAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Product movement history retrieved successfully.", result.Message);
        Assert.Single(result.Data!);

        // Verify
        movementRepo.Verify(repo => repo.GetMovementsByProductIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetUserMovementHistory_MissingUser_Returns404()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        userRepo.Setup(repo => repo.GetUserByIdAsync(99)).ReturnsAsync((User?)null);
        var service = CreateService(movementRepo, productRepo, userRepo);

        // Act
        var result = await service.GetMovementsByUserIdAsync(99);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("User not found.", result.Message);

        // Verify
        movementRepo.Verify(repo => repo.GetMovementsByUserIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetMovementsByDateRange_StartAfterEnd_Returns400()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        var service = CreateService(movementRepo, productRepo, userRepo);
        var start = DateTime.UtcNow;

        // Act
        var result = await service.GetMovementsByDateRangeAsync(start, start.AddDays(-1));

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Start date cannot be later than end date.", result.Message);

        // Verify
        movementRepo.Verify(
            repo => repo.GetMovementsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMovementsByDateRange_MatchingMovements_Returns200()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        var start = DateTime.UtcNow.AddDays(-2);
        var end = DateTime.UtcNow;
        movementRepo.Setup(repo => repo.GetMovementsByDateRangeAsync(start, end))
            .ReturnsAsync([CreateMovement()]);
        var service = CreateService(movementRepo, productRepo, userRepo);

        // Act
        var result = await service.GetMovementsByDateRangeAsync(start, end);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Movements retrieved successfully for the specified date range.", result.Message);
        Assert.Single(result.Data!);

        // Verify
        movementRepo.Verify(repo => repo.GetMovementsByDateRangeAsync(start, end), Times.Once);
    }

    [Fact]
    public async Task GetMovementsByType_InvalidType_Returns400()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        var service = CreateService(movementRepo, productRepo, userRepo);

        // Act
        var result = await service.GetMovementsByMovementTypeAsync((MovementType)999);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Invalid movement type.", result.Message);

        // Verify
        movementRepo.Verify(repo => repo.GetMovementsByTypeAsync(It.IsAny<MovementType>()), Times.Never);
    }

    [Fact]
    public async Task GetMovementsByType_MatchingMovements_Returns200()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        movementRepo.Setup(repo => repo.GetMovementsByTypeAsync(MovementType.StockIn))
            .ReturnsAsync([CreateMovement()]);
        var service = CreateService(movementRepo, productRepo, userRepo);

        // Act
        var result = await service.GetMovementsByMovementTypeAsync(MovementType.StockIn);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Movements retrieved successfully for the specified movement type.", result.Message);
        Assert.Single(result.Data!);

        // Verify
        movementRepo.Verify(repo => repo.GetMovementsByTypeAsync(MovementType.StockIn), Times.Once);
    }

    [Fact]
    public async Task RecordMovement_NonPositiveQuantity_Returns400()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        var service = CreateService(movementRepo, productRepo, userRepo);
        var request = CreateRequest(MovementType.StockIn);
        request.Quantity = 0;

        // Act
        var result = await service.RecordStockInAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Quantity of movement must be greater than zero.", result.Message);

        // Verify
        movementRepo.Verify(repo => repo.AddMovementAsync(It.IsAny<InventoryMovement>()), Times.Never);
    }

    [Fact]
    public async Task RecordStockIn_ValidRequest_IncreasesStockAndReturns201()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        var product = CreateProduct();

        // Movement recording requires an existing active product and user.
        // This helper configures all four repository answers.
        ConfigureActiveProductAndUser(productRepo, userRepo, product);

        // Return the same movement received from the service. This behaves like a
        // simple repository insert without requiring a database-generated result.
        movementRepo.Setup(repo => repo.AddMovementAsync(It.IsAny<InventoryMovement>()))
            .ReturnsAsync((InventoryMovement movement) => movement);
        var service = CreateService(movementRepo, productRepo, userRepo);

        // Act
        var result = await service.RecordStockInAsync(CreateRequest(MovementType.StockIn));

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal("Movement recorded successfully.", result.Message);

        // Stock began at 10 and the request adds 5.
        Assert.Equal(15, result.Data!.QuantityAfter);

        // The tracked Product entity must be updated as well as the response.
        Assert.Equal(15, product.QuantityInStock);

        // It.Is<InventoryMovement>(...) verifies the values of the exact movement
        // object passed into AddMovementAsync, not only that the method was called.
        movementRepo.Verify(repo => repo.AddMovementAsync(
            It.Is<InventoryMovement>(movement =>
                movement.QuantityBefore == 10 && movement.QuantityAfter == 15)), Times.Once);
        productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RecordStockOut_ValidRequest_DecreasesStockAndReturns201()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        var product = CreateProduct();
        ConfigureActiveProductAndUser(productRepo, userRepo, product);
        movementRepo.Setup(repo => repo.AddMovementAsync(It.IsAny<InventoryMovement>()))
            .ReturnsAsync((InventoryMovement movement) => movement);
        var service = CreateService(movementRepo, productRepo, userRepo);
        var request = CreateRequest(MovementType.StockOut);
        request.Quantity = 3;

        // Act
        var result = await service.RecordStockOutAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Movement recorded successfully.", result.Message);
        Assert.Equal(7, result.Data!.QuantityAfter);
        Assert.Equal(7, product.QuantityInStock);

        // Verify
        movementRepo.Verify(repo => repo.AddMovementAsync(It.IsAny<InventoryMovement>()), Times.Once);
        productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RecordStockOut_InsufficientStock_Returns400WithoutSaving()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        var product = CreateProduct();
        ConfigureActiveProductAndUser(productRepo, userRepo, product);
        var service = CreateService(movementRepo, productRepo, userRepo);
        var request = CreateRequest(MovementType.StockOut);
        request.Quantity = 11;

        // Act
        var result = await service.RecordStockOutAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Insufficient stock for the requested movement.", result.Message);
        Assert.Equal(10, product.QuantityInStock);

        // Verify
        movementRepo.Verify(repo => repo.AddMovementAsync(It.IsAny<InventoryMovement>()), Times.Never);
        productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task RecordStockIn_MissingProduct_Returns404()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        productRepo.Setup(repo => repo.GetProductAsync(1)).ReturnsAsync((Product?)null);
        var service = CreateService(movementRepo, productRepo, userRepo);

        // Act
        var result = await service.RecordStockInAsync(CreateRequest(MovementType.StockIn));

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Product not found.", result.Message);

        // Verify
        movementRepo.Verify(repo => repo.AddMovementAsync(It.IsAny<InventoryMovement>()), Times.Never);
    }

    [Fact]
    public async Task RecordStockIn_InactiveUser_Returns400()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        var product = CreateProduct();
        productRepo.Setup(repo => repo.GetProductAsync(1)).ReturnsAsync(product);
        productRepo.Setup(repo => repo.IsProductActiveAsync(1)).ReturnsAsync(true);
        userRepo.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(CreateUser());
        userRepo.Setup(repo => repo.IsUserActiveAsync(1)).ReturnsAsync(false);
        var service = CreateService(movementRepo, productRepo, userRepo);

        // Act
        var result = await service.RecordStockInAsync(CreateRequest(MovementType.StockIn));

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("User is not active.", result.Message);

        // Verify
        movementRepo.Verify(repo => repo.AddMovementAsync(It.IsAny<InventoryMovement>()), Times.Never);
    }

    [Fact]
    public async Task RecordAdjustment_DecreaseWithEnoughStock_Returns201()
    {
        // Arrange
        var (movementRepo, productRepo, userRepo) = CreateRepositories();
        var product = CreateProduct();
        ConfigureActiveProductAndUser(productRepo, userRepo, product);
        movementRepo.Setup(repo => repo.AddMovementAsync(It.IsAny<InventoryMovement>()))
            .ReturnsAsync((InventoryMovement movement) => movement);
        var service = CreateService(movementRepo, productRepo, userRepo);
        var request = CreateRequest(MovementType.AdjustmentDecrease);
        request.Quantity = 2;

        // Act
        var result = await service.RecordAdjustmentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Movement recorded successfully.", result.Message);
        Assert.Equal(8, result.Data!.QuantityAfter);

        // Verify
        movementRepo.Verify(repo => repo.AddMovementAsync(It.IsAny<InventoryMovement>()), Times.Once);
        productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    private static InventoryMovementService CreateService(
        Mock<IInventoryMovementRepository> movementRepository,
        Mock<IProductRepository> productRepository,
        Mock<IUserRepository> userRepository)
    {
        // The service is real; only its external dependencies are mocked.
        return new InventoryMovementService(
            movementRepository.Object, productRepository.Object, userRepository.Object);
    }

    private static (Mock<IInventoryMovementRepository>, Mock<IProductRepository>, Mock<IUserRepository>)
        CreateRepositories()
    {
        // Returning the three mocks as a tuple keeps each test's Arrange section short.
        return (new Mock<IInventoryMovementRepository>(),
            new Mock<IProductRepository>(),
            new Mock<IUserRepository>());
    }

    private static void ConfigureActiveProductAndUser(
        Mock<IProductRepository> productRepository,
        Mock<IUserRepository> userRepository,
        Product product)
    {
        // GetProduct/GetUser prove the records exist. The separate active checks
        // reproduce the additional business rules used by the service.
        productRepository.Setup(repo => repo.GetProductAsync(1)).ReturnsAsync(product);
        productRepository.Setup(repo => repo.IsProductActiveAsync(1)).ReturnsAsync(true);
        userRepository.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(CreateUser());
        userRepository.Setup(repo => repo.IsUserActiveAsync(1)).ReturnsAsync(true);
    }

    private static Product CreateProduct()
    {
        // QuantityInStock starts at 10 to make stock calculations easy to read.
        return new Product
        {
            ID = 1,
            Sku = "SKU00001",
            Name = "Test Product",
            Description = "Test product description",
            CategoryID = 1,
            SupplierID = 1,
            QuantityInStock = 10,
            IsActive = true
        };
    }

    private static User CreateUser()
    {
        return new User
        {
            ID = 1,
            UserName = "TestUser",
            Email = "test@example.com",
            Password_Hash = "not-used",
            IsActive = true
        };
    }

    private static InventoryMovement CreateMovement()
    {
        // Product and User are populated because retrieval methods map their
        // navigation properties into response DTO fields.
        return new InventoryMovement
        {
            ID = 1,
            ProductId = 1,
            Product = CreateProduct(),
            Quantity = 5,
            QuantityBefore = 10,
            QuantityAfter = 15,
            Movement = MovementType.StockIn,
            UserID = 1,
            User = CreateUser(),
            Reason = "Restock"
        };
    }

    private static CreateInventoryMovementRequestDTO CreateRequest(MovementType type)
    {
        // Valid baseline movement request. Tests alter only quantity or movement type.
        return new CreateInventoryMovementRequestDTO
        {
            ProductId = 1,
            Quantity = 5,
            Movement = type,
            UserID = 1,
            Reason = "Inventory update"
        };
    }
}
