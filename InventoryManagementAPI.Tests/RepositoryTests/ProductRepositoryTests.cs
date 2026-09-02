using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.Contracts.Products;
using InventoryManagementAPI.Repositorys.ProductRepositories;

namespace InventoryManagementAPI.Tests.RepositoryTests;

public class ProductRepositoryTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;

    public ProductRepositoryTests(SqlServerFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetProductsAsync_PaginatesAndReturnsFilteredTotal()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var products = await AddQueryProductsAsync(context);
        context.ChangeTracker.Clear();

        var result = await new ProductRepository(context).GetProductsAsync(
            new ProductQueryParameters
            {
                CategoryId = products[0].CategoryID,
                Page = 2,
                PageSize = 2,
                SortBy = "name",
                SortDirection = "asc"
            },
            CancellationToken.None);

        Assert.Equal(5, result.TotalItems);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(
            products.OrderBy(product => product.Name).ThenBy(product => product.ID).Skip(2).Take(2).Select(product => product.ID),
            result.Items.Select(product => product.ID));
    }

    [Fact]
    public async Task GetProductsAsync_SearchMatchesNameOrSku()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var products = await AddQueryProductsAsync(context);
        var target = products[2];
        context.ChangeTracker.Clear();
        var repository = new ProductRepository(context);

        var nameResult = await repository.GetProductsAsync(
            new ProductQueryParameters { Search = target.Name, PageSize = 100 },
            CancellationToken.None);
        var skuResult = await repository.GetProductsAsync(
            new ProductQueryParameters { Search = target.Sku, PageSize = 100 },
            CancellationToken.None);

        Assert.Equal(1, nameResult.TotalItems);
        Assert.Equal(target.ID, Assert.Single(nameResult.Items).ID);
        Assert.Equal(1, skuResult.TotalItems);
        Assert.Equal(target.ID, Assert.Single(skuResult.Items).ID);
    }

    [Fact]
    public async Task GetProductsAsync_CombinesCategoryStatusAndPriceFilters()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var products = await AddQueryProductsAsync(context);
        context.ChangeTracker.Clear();

        var result = await new ProductRepository(context).GetProductsAsync(
            new ProductQueryParameters
            {
                CategoryId = products[0].CategoryID,
                IsActive = true,
                MinPrice = 20m,
                MaxPrice = 40m,
                PageSize = 100
            },
            CancellationToken.None);

        Assert.Equal(2, result.TotalItems);
        Assert.Equal(
            products.Where(product => product.IsActive && product.Price is >= 20m and <= 40m).Select(product => product.ID).Order(),
            result.Items.Select(product => product.ID).Order());
    }

    [Fact]
    public async Task GetProductsAsync_SortsByPriceDescendingWithStableIdTieBreaker()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var products = await AddQueryProductsAsync(context);
        context.ChangeTracker.Clear();

        var result = await new ProductRepository(context).GetProductsAsync(
            new ProductQueryParameters
            {
                CategoryId = products[0].CategoryID,
                SortBy = "price",
                SortDirection = "desc",
                PageSize = 100
            },
            CancellationToken.None);

        Assert.Equal(
            products.OrderByDescending(product => product.Price).ThenBy(product => product.ID).Select(product => product.ID),
            result.Items.Select(product => product.ID));
    }

    [Fact]
    public async Task GetProductsAsync_NoMatches_ReturnsEmptyPageAndZeroTotal()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var products = await AddQueryProductsAsync(context);
        context.ChangeTracker.Clear();

        var result = await new ProductRepository(context).GetProductsAsync(
            new ProductQueryParameters
            {
                CategoryId = products[0].CategoryID,
                Search = $"missing-{Guid.NewGuid():N}"
            },
            CancellationToken.None);

        Assert.Equal(0, result.TotalItems);
        Assert.Empty(result.Items);
    }

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
        var products = await AddQueryProductsAsync(context);
        await AddProductAsync(context);
        context.ChangeTracker.Clear();

        var result = await new ProductRepository(context).GetProductsByCategoryAsync(
            products[0].CategoryID,
            new ProductQueryParameters
            {
                Page = 2,
                PageSize = 2,
                SortBy = "name",
                SortDirection = "asc"
            },
            CancellationToken.None);

        Assert.Equal(5, result.TotalItems);
        Assert.Equal(
            products.OrderBy(product => product.Name).ThenBy(product => product.ID).Skip(2).Take(2).Select(product => product.ID),
            result.Items.Select(product => product.ID));
        Assert.All(result.Items, candidate => Assert.Equal(products[0].CategoryID, candidate.CategoryID));
    }

    [Fact]
    public async Task GetProductsBelowReorderLevelAsync_UsesWarehouseStock()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var belowReorderLevel = await AddProductAsync(context, quantity: 4, reorderLevel: 5);
        var atReorderLevel = await AddProductAsync(context, quantity: 5, reorderLevel: 5);
        context.ChangeTracker.Clear();

        var result = await new ProductRepository(context).GetProductsBelowReorderLevelAsync(
            new ProductQueryParameters { Page = 1, PageSize = 100 },
            CancellationToken.None);

        Assert.Equal(1, result.TotalItems);
        Assert.Equal(belowReorderLevel.ID, Assert.Single(result.Items).ID);
        Assert.DoesNotContain(result.Items, candidate => candidate.ID == atReorderLevel.ID);
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

 private static async Task<Product> AddProductAsync(Services.InvManDBContext context, int quantity = 10, int reorderLevel = 5)
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

    private static async Task<List<Product>> AddQueryProductsAsync(Services.InvManDBContext context)
    {
        var category = await AddCategoryAsync(context);
        var unique = Guid.NewGuid().ToString("N")[..6];
        var products = new List<Product>
        {
            CreateQueryProduct(category.ID, $"Alpha {unique}", $"A{unique}1", 10m, true, 1),
            CreateQueryProduct(category.ID, $"Bravo {unique}", $"B{unique}2", 20m, false, 2),
            CreateQueryProduct(category.ID, $"Charlie {unique}", $"C{unique}3", 30m, true, 3),
            CreateQueryProduct(category.ID, $"Delta {unique}", $"D{unique}4", 30m, true, 4),
            CreateQueryProduct(category.ID, $"Echo {unique}", $"E{unique}5", 50m, false, 5)
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync(CancellationToken.None);
        return products;
    }

    private static Product CreateQueryProduct(
        int categoryId,
        string name,
        string sku,
        decimal price,
        bool isActive,
        int createdOffset) => new()
    {
        Sku = sku,
        Name = name,
        Description = "Product used to test querying, filtering, sorting and pagination",
        CategoryID = categoryId,
        Price = price,
        IsActive = isActive,
        Created = DateTime.UtcNow.AddMinutes(createdOffset),
        Updated = DateTime.UtcNow.AddMinutes(createdOffset)
    };

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
