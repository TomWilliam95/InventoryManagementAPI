using InventoryManagementAPI.Models.Contracts.InventoryStocks;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.InventoryStockDTO_s;
using InventoryManagementAPI.Repositories.InventoryStockRepositories;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using InventoryManagementAPI.Repositories.WarehouseRepositories;
using Moq;
using InventoryManagementAPI.Models.Shared;

namespace InventoryManagementAPI.Tests;

public class InventoryStockServiceTests
{
    private readonly Mock<IInventoryStockRepository> _stocks = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<IWarehouseRepository> _warehouses = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private InventoryStockService CreateService() => new(_stocks.Object, _products.Object, _warehouses.Object, _unitOfWork.Object);

    [Theory]
    [InlineData(4, true)]
    [InlineData(5, true)]
    [InlineData(6, false)]
    public async Task GetInventoryStocks_MapsDetailsAndPreservesPagination(int quantity, bool below)
    {
        var query = new InventoryStockQueryParameters
        {
            Page = 2, PageSize = 1, ProductId = 1, WarehouseId = 1,
            Search = "Product", IsActive = true, BelowReorderLevel = true,
            SortBy = "warehouse", SortDirection = "desc"
        };
        using var source = new CancellationTokenSource();
        _stocks.Setup(r => r.GetStockAsync(query, source.Token))
            .ReturnsAsync(new PagedData<InventoryStock>
            {
                Items = [CreateStock(quantity, 5)], TotalItems = 3
            });

        var result = await CreateService().GetInventoryStocksAsync(query, source.Token);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        var page = Assert.IsType<PagedResult<BulkInventoryStockResponseDTO>>(result.Data);
        var item = Assert.Single(page.Items);
        Assert.Equal("SKU-1", item.ProductSku);
        Assert.Equal("Warehouse", item.WarehouseName);
        Assert.Equal(below, item.IsBelowReorderLevel);
        Assert.Equal(2, page.Page);
        Assert.Equal(1, page.PageSize);
        Assert.Equal(3, page.TotalItems);
        Assert.Equal(3, page.TotalPages);
        Assert.True(page.HasNextPage);
        _stocks.Verify(r => r.GetStockAsync(query, source.Token), Times.Once);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(3, 25)]
    public async Task GetInventoryStocks_EmptyPage_PreservesTotal(int pageNumber, int total)
    {
        var query = new InventoryStockQueryParameters { Page = pageNumber, PageSize = 20 };
        _stocks.Setup(r => r.GetStockAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedData<InventoryStock> { Items = [], TotalItems = total });

        var result = await CreateService().GetInventoryStocksAsync(query);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Empty(result.Data!.Items);
        Assert.Equal(pageNumber, result.Data.Page);
        Assert.Equal(20, result.Data.PageSize);
        Assert.Equal(total, result.Data.TotalItems);
    }

    [Fact]
    public async Task GetInventoryStocks_RepositoryFailure_Returns500()
    {
        _stocks.Setup(r => r.GetStockAsync(It.IsAny<InventoryStockQueryParameters>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));
        var result = await CreateService().GetInventoryStocksAsync(new InventoryStockQueryParameters());
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Null(result.Data);
    }
    [Fact]
    public async Task UpdateReorderLevel_StaleRowVersion_Returns409WithoutSaving()
    {
        _stocks.Setup(repository => repository.GetStockByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateStock());

        var result = await CreateService().UpdateReorderLevelAsync(1, new UpdateReorderLevelRequestDTO
        {
            ReorderLevel = 20,
            RowVersion = [9, 9, 9, 9, 9, 9, 9, 9]
        });

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateInventoryStock_ValidRequest_AssignsDetailsAndReturns201()
    {
        var product = CreateProduct();
        var warehouse = CreateWarehouse();
        _products.Setup(repository => repository.GetProductAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _warehouses.Setup(repository => repository.GetWarehouseByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);
        _stocks.Setup(repository => repository.GetStockByProductAndWarehouseIDAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryStock?)null);
        _stocks.Setup(repository => repository.CreateInventoryStockAsync(It.IsAny<InventoryStock>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryStock stock, CancellationToken _) => stock);

        var result = await CreateService().CreateInventoryStockAsync(new CreateInventoryStockRequestDTO
        {
            ProductID = 1,
            WarehouseID = 1,
            ReorderLevel = 10
        });

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal(product.Sku, result.Data!.ProductSku);
        Assert.Equal(warehouse.Name, result.Data.WarehouseName);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetInventoryStocks_CancelledOperation_RethrowsCancellation()
    {
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        _stocks.Setup(repository => repository.GetStockAsync(It.IsAny<InventoryStockQueryParameters>(), cancellationSource.Token))
            .ThrowsAsync(new OperationCanceledException(cancellationSource.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            CreateService().GetInventoryStocksAsync(new InventoryStockQueryParameters(), cancellationSource.Token));
    }

    private static InventoryStock CreateStock(int quantity = 10, int reorderLevel = 5) => new()
    {
        ID = 1,
        ProductID = 1,
        Product = CreateProduct(),
        WarehouseID = 1,
        Warehouse = CreateWarehouse(),
        Quantity = quantity,
        ReorderLevel = reorderLevel,
        Created = DateTime.UtcNow,
        Updated = DateTime.UtcNow,
        IsActive = true,
        RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
    };

    private static Product CreateProduct() => new()
    {
        ID = 1,
        Sku = "SKU-1",
        Name = "Product",
        Description = "Description",
        CategoryID = 1,
        Price = 10,
        IsActive = true
    };

    private static Warehouse CreateWarehouse() => new()
    {
        ID = 1,
        Name = "Warehouse",
        Address = "1 Test Street",
        City = "Brisbane",
        State = "QLD",
        ZipCode = "4000",
        Country = "Australia",
        IsActive = true
    };
}
