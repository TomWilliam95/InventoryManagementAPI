using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.CoreModels.MovementModels;
using InventoryManagementAPI.Models.DTO_s.MovementDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.InventoryStockRepositories;
using InventoryManagementAPI.Repositories.InvMovementRepositories;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using InventoryManagementAPI.Repositories.UserRepositories;
using InventoryManagementAPI.Repositories.WarehouseRepositories;
using Moq;

namespace InventoryManagementAPI.Tests;

public class InventoryMovementServiceTests
{
    private readonly Mock<IInventoryMovementRepository> _movements = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IWarehouseRepository> _warehouses = new();
    private readonly Mock<IInventoryStockRepository> _stocks = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private InventoryMovementService CreateService() => new(
        _movements.Object,
        _products.Object,
        _users.Object,
        _warehouses.Object,
        _stocks.Object,
        _unitOfWork.Object);

    [Fact]
    public async Task GetMovementById_ExistingMovement_Returns200()
    {
        _movements.Setup(repository => repository.GetMovementByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMovement());

        var result = await CreateService().GetMovementByIdAsync(1);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Test Product", result.Data!.ProductName);
        Assert.Equal("Main Warehouse", result.Data.WarehouseName);
    }

    [Fact]
    public async Task GetMovementById_MissingMovement_Returns404()
    {
        _movements.Setup(repository => repository.GetMovementByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryMovement?)null);

        var result = await CreateService().GetMovementByIdAsync(99);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task GetAllMovements_NoMovements_Returns404()
    {
        _movements.Setup(repository => repository.GetAllMovementsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await CreateService().GetAllMovementsAsync();

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task RecordStockIn_ValidRequest_IncreasesWarehouseStock()
    {
        var stock = ConfigureValidDependencies();
        _movements.Setup(repository => repository.AddMovementAsync(It.IsAny<InventoryMovement>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryMovement movement, CancellationToken _) => movement);

        var result = await CreateService().RecordStockInAsync(CreateRequest(MovementType.StockIn), 1);

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal(15, stock.Quantity);
        Assert.Equal(15, result.Data!.QuantityAfter);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordStockOut_ValidRequest_DecreasesWarehouseStock()
    {
        var stock = ConfigureValidDependencies();
        var request = CreateRequest(MovementType.StockOut);
        request.Quantity = 3;

        var result = await CreateService().RecordStockOutAsync(request, 1);

        Assert.True(result.Success);
        Assert.Equal(7, stock.Quantity);
        Assert.Equal(7, result.Data!.QuantityAfter);
    }

    [Fact]
    public async Task RecordStockOut_InsufficientStock_Returns400WithoutSaving()
    {
        ConfigureValidDependencies();
        var request = CreateRequest(MovementType.StockOut);
        request.Quantity = 11;

        var result = await CreateService().RecordStockOutAsync(request, 1);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Insufficient stock for the requested movement.", result.Message);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RecordStockIn_NonPositiveQuantity_Returns400BeforeRepositories()
    {
        var request = CreateRequest(MovementType.StockIn);
        request.Quantity = 0;

        var result = await CreateService().RecordStockInAsync(request, 1);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        _users.Verify(repository => repository.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RecordStockIn_MissingWarehouse_Returns404()
    {
        var product = CreateProduct();
        _users.Setup(repository => repository.GetUserByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateUser());
        _users.Setup(repository => repository.IsUserActiveAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _products.Setup(repository => repository.GetProductAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _products.Setup(repository => repository.IsProductActiveAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _warehouses.Setup(repository => repository.GetWarehouseByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Warehouse?)null);

        var result = await CreateService().RecordStockInAsync(CreateRequest(MovementType.StockIn), 1);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Warehouse not found.", result.Message);
    }

    [Fact]
    public async Task RecordAdjustment_Decrease_UpdatesWarehouseStock()
    {
        var stock = ConfigureValidDependencies();
        var request = CreateRequest(MovementType.AdjustmentDecrease);
        request.Quantity = 2;

        var result = await CreateService().RecordAdjustmentAsync(request, 1);

        Assert.True(result.Success);
        Assert.Equal(8, stock.Quantity);
    }

    private InventoryStock ConfigureValidDependencies()
    {
        var product = CreateProduct();
        var warehouse = CreateWarehouse();
        var stock = new InventoryStock
        {
            ID = 1,
            ProductID = product.ID,
            Product = product,
            WarehouseID = warehouse.ID,
            Warehouse = warehouse,
            Quantity = 10,
            ReorderLevel = 5,
            IsActive = true,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow
        };

        _users.Setup(repository => repository.GetUserByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateUser());
        _users.Setup(repository => repository.IsUserActiveAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _products.Setup(repository => repository.GetProductAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _products.Setup(repository => repository.IsProductActiveAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _warehouses.Setup(repository => repository.GetWarehouseByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);
        _warehouses.Setup(repository => repository.IsWarehouseActiveAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _stocks.Setup(repository => repository.GetStockByProductAndWarehouseIDAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(stock);
        _movements.Setup(repository => repository.AddMovementAsync(It.IsAny<InventoryMovement>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryMovement movement, CancellationToken _) => movement);
        return stock;
    }

    private static InventoryMovement CreateMovement()
    {
        var stock = new InventoryStock
        {
            ID = 1,
            ProductID = 1,
            Product = CreateProduct(),
            WarehouseID = 1,
            Warehouse = CreateWarehouse(),
            Quantity = 15,
            ReorderLevel = 5,
            IsActive = true
        };
        return new InventoryMovement
        {
            ID = 1,
            InventoryStockID = stock.ID,
            InventoryStock = stock,
            UserID = 1,
            User = CreateUser(),
            Quantity = 5,
            QuantityBefore = 10,
            QuantityAfter = 15,
            Movement = MovementType.StockIn,
            Reason = "Restock"
        };
    }

    private static Product CreateProduct() => new()
    {
        ID = 1,
        Sku = "SKU00001",
        Name = "Test Product",
        Description = "Test product description",
        CategoryID = 1,
        Price = 10m,
        IsActive = true
    };

    private static Warehouse CreateWarehouse() => new()
    {
        ID = 1,
        Name = "Main Warehouse",
        Address = "1 Test Street",
        City = "Brisbane",
        State = "QLD",
        ZipCode = "4000",
        Country = "Australia",
        IsActive = true,
        Created = DateTime.UtcNow,
        Updated = DateTime.UtcNow
    };

    private static User CreateUser() => new()
    {
        ID = 1,
        UserName = "TestUser",
        Email = "test@example.com",
        Password_Hash = "not-used",
        IsActive = true
    };

    private static CreateInventoryMovementRequestDTO CreateRequest(MovementType movement) => new()
    {
        ProductID = 1,
        WarehouseID = 1,
        Quantity = 5,
        Movement = movement,
        Reason = "Inventory update"
    };
}
