using InventoryManagementAPI.Models.Contracts.InventoryStocks;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Repositories.InventoryStockRepositories;
using InventoryManagementAPI.Services;

namespace InventoryManagementAPI.Tests.RepositoryTests;

public class InventoryStockRepositoryTests(SqlServerFixture fixture) : IClassFixture<SqlServerFixture>
{
    [Theory]
    [InlineData("product", "asc", new[] { 1, 3, 0, 2 })]
    [InlineData("product", "desc", new[] { 0, 2, 1, 3 })]
    [InlineData("warehouse", "asc", new[] { 2, 3, 0, 1 })]
    [InlineData("warehouse", "desc", new[] { 0, 1, 2, 3 })]
    [InlineData("created", "asc", new[] { 1, 2, 3, 0 })]
    [InlineData("created", "desc", new[] { 0, 3, 1, 2 })]
    [InlineData("id", "asc", new[] { 0, 1, 2, 3 })]
    [InlineData("id", "desc", new[] { 3, 2, 1, 0 })]
    public async Task GetStock_SortsWithIdTieBreaker(string sort, string direction, int[] expected)
    {
        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var stocks = await SeedAsync(context);
        var result = await new InventoryStockRepository(context).GetStockAsync(
            new InventoryStockQueryParameters { SortBy = sort, SortDirection = direction }, CancellationToken.None);
        Assert.Equal(4, result.TotalItems);
        Assert.Equal(expected.Select(i => stocks[i].ID), result.Items.Select(s => s.ID));
        Assert.All(result.Items, s => { Assert.NotNull(s.Product); Assert.NotNull(s.Warehouse); });
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Theory]
    [InlineData(null, null, null, new[] { 0, 1, 2, 3 })]
    [InlineData("   ", null, false, new[] { 0, 1, 2, 3 })]
    [InlineData(null, null, true, new[] { 0, 1, 3 })]
    [InlineData("  ALPHA  ", null, null, new[] { 1, 3 })]
    [InlineData("  NORTH  ", null, null, new[] { 2, 3 })]
    [InlineData(null, true, null, new[] { 0, 2, 3 })]
    [InlineData(null, false, null, new[] { 1 })]
    [InlineData("missing", null, null, new int[] { })]
    public async Task GetStock_FiltersSearchStatusAndReorder(string? search, bool? active, bool? below, int[] expected)
    {
        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var stocks = await SeedAsync(context);
        var result = await new InventoryStockRepository(context).GetStockAsync(new InventoryStockQueryParameters
        {
            Search = search, IsActive = active, BelowReorderLevel = below, SortBy = "id"
        }, CancellationToken.None);
        Assert.Equal(expected.Length, result.TotalItems);
        Assert.Equal(expected.Select(i => stocks[i].ID), result.Items.Select(s => s.ID));
    }

    [Theory]
    [InlineData(true, false, new[] { 0, 2 })]
    [InlineData(false, true, new[] { 0, 1 })]
    [InlineData(true, true, new[] { 0 })]
    public async Task GetStock_FiltersProductAndWarehouse(bool product, bool warehouse, int[] expected)
    {
        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var stocks = await SeedAsync(context);
        var result = await new InventoryStockRepository(context).GetStockAsync(new InventoryStockQueryParameters
        {
            ProductId = product ? stocks[0].ProductID : null,
            WarehouseId = warehouse ? stocks[0].WarehouseID : null, SortBy = "id"
        }, CancellationToken.None);
        Assert.Equal(expected.Length, result.TotalItems);
        Assert.Equal(expected.Select(i => stocks[i].ID), result.Items.Select(s => s.ID));
    }

    [Theory]
    [InlineData(1, new[] { 0 })]
    [InlineData(2, new[] { 3 })]
    [InlineData(3, new int[] { })]
    public async Task GetStock_CombinedFiltersBeforePaging_PreservesTotal(int page, int[] expected)
    {
        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var stocks = await SeedAsync(context);
        var result = await new InventoryStockRepository(context).GetStockAsync(new InventoryStockQueryParameters
        {
            IsActive = true, BelowReorderLevel = true, SortBy = "product", SortDirection = "desc",
            Page = page, PageSize = 1
        }, CancellationToken.None);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(expected.Select(i => stocks[i].ID), result.Items.Select(s => s.ID));
    }

    private static async Task<InventoryStock[]> SeedAsync(InvManDBContext context)
    {
        var date = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var category = new Category { Name = "Stock query category", Created = date, Updated = date, IsActive = true };
        var products = new[] { "Zulu", "Alpha" }.Select((name, i) => new Product
        {
            Name = name, Sku = $"STOCK00{i}", Description = "Stock query product", Category = category,
            Price = 10, IsActive = true, Created = date, Updated = date
        }).ToArray();
        var warehouses = new[] { "South", "North" }.Select(name => new Warehouse
        {
            Name = name, Address = "1 Test Street", City = "Brisbane", State = "QLD",
            ZipCode = "4000", Country = "Australia", IsActive = true, Created = date, Updated = date
        }).ToArray();
        var stocks = new InventoryStock[4];
        var quantities = new[] { 4, 5, 6, 0 };
        var days = new[] { 2, 0, 0, 1 };
        for (var i = 0; i < stocks.Length; i++)
        {
            stocks[i] = new InventoryStock
            {
                Product = products[i % 2], Warehouse = warehouses[i / 2], Quantity = quantities[i],
                ReorderLevel = 5, IsActive = i != 1, Created = date.AddDays(days[i]), Updated = date
            };
            // Save separately to make identity ordering deterministic.
            context.InventoryStocks.Add(stocks[i]);
            await context.SaveChangesAsync();
        }
        context.ChangeTracker.Clear();
        return stocks;
    }
}
