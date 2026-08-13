using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Repositorys.ProductRepositories;

namespace InventoryManagementAPI.Tests.RepositoryTests;

public class ProductRepositoryTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;

    public ProductRepositoryTests(SqlServerFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetProductAsync_ExistingProduct_ReturnsProductWithRelationships()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var product = await AddProductAsync(context);
        context.ChangeTracker.Clear();

        var result = await new ProductRepository(context).GetProductAsync(product.ID, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Category);
        Assert.NotEmpty(result.InventoryStocks);
    }

    [Fact]
    public async Task GetProductAsync_MissingProduct_ReturnsNull()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

        var result = await new ProductRepository(context).GetProductAsync(int.MaxValue, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_ReturnsOnlyMatchingProducts()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var product = await AddProductAsync(context);
        context.ChangeTracker.Clear();

        var result = (await new ProductRepository(context)
            .GetProductsByCategoryAsync(product.CategoryID, CancellationToken.None)).ToList();

        Assert.Contains(result, candidate => candidate.ID == product.ID);
        Assert.All(result, candidate => Assert.Equal(product.CategoryID, candidate.CategoryID));
    }

    [Fact]
    public async Task GetProductsBelowReorderLevelAsync_UsesWarehouseStock()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var product = await AddProductAsync(context, quantity: 4, reorderLevel: 5);
        context.ChangeTracker.Clear();

        var result = await new ProductRepository(context).GetProductsBelowReorderLevelAsync(CancellationToken.None);

        Assert.Contains(result, candidate => candidate.ID == product.ID);
    }

    [Fact]
    public async Task AddProductAsync_TracksProductUntilUnitOfWorkSaves()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var category = await AddCategoryAsync(context);
        var product = CreateProduct(category.ID);
        var repository = new ProductRepository(context);

        var result = await repository.AddProductAsync(product, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);
        context.ChangeTracker.Clear();

        Assert.NotNull(await context.Products.FindAsync([result.ID], CancellationToken.None));
    }

    [Fact]
    public async Task ProductExistenceQueries_RespectCurrentProductId()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var product = await AddProductAsync(context);
        var repository = new ProductRepository(context);

        Assert.True(await repository.ProductExistsAsync(product.ID, CancellationToken.None));
        Assert.True(await repository.ProductNameExistsAsync(product.Name, CancellationToken.None));
        Assert.True(await repository.ProductSkuExistsAsync(product.Sku, CancellationToken.None));
        Assert.False(await repository.OtherProductNameExistsAsync(product.ID, product.Name, CancellationToken.None));
        Assert.False(await repository.OtherProductSkuExistsAsync(product.ID, product.Sku, CancellationToken.None));
    }

    private static async Task<Product> AddProductAsync(
        Services.InvManDBContext context,
        int quantity = 10,
        int reorderLevel = 5)
    {
        var category = await AddCategoryAsync(context);
        var warehouse = new Warehouse
        {
            Name = $"Warehouse {Guid.NewGuid():N}",
            Address = "1 Test Street",
            City = "Brisbane",
            State = "QLD",
            ZipCode = "4000",
            Country = "Australia",
            IsActive = true,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow
        };
        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync(CancellationToken.None);

        var product = CreateProduct(category.ID);
        product.InventoryStocks.Add(new InventoryStock
        {
            WarehouseID = warehouse.ID,
            Quantity = quantity,
            ReorderLevel = reorderLevel,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow
        });
        context.Products.Add(product);
        await context.SaveChangesAsync(CancellationToken.None);
        return product;
    }

    private static async Task<Category> AddCategoryAsync(Services.InvManDBContext context)
    {
        var category = new Category
        {
            Name = $"Category {Guid.NewGuid():N}",
            Description = "Category for product repository tests"
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync(CancellationToken.None);
        return category;
    }

    private static Product CreateProduct(int categoryId) => new()
    {
        Sku = $"SKU{Guid.NewGuid():N}"[..8],
        Name = $"Product {Guid.NewGuid():N}",
        Description = "Product used by repository tests",
        CategoryID = categoryId,
        Price = 10m,
        IsActive = true,
        Created = DateTime.UtcNow,
        Updated = DateTime.UtcNow
    };
}
