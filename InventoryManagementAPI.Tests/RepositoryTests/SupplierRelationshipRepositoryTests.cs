using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Repositories.SupplierAddressRepositories;
using InventoryManagementAPI.Repositories.SupplierContactRepositories;
using InventoryManagementAPI.Repositories.SupplierProductRepositories;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Tests.RepositoryTests;

public class SupplierRelationshipRepositoryTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;
    public SupplierRelationshipRepositoryTests(SqlServerFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task SupplierContactRepository_AddAndOwnedLookup_ReturnsContact()
    {
        await using var context = _fixture.CreateContext(); await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var supplier = await AddSupplierAsync(context); var repository = new SupplierContactRepository(context);
        var contact = new SupplierContact { SupplierID = supplier.ID, Name = "Buyer", Email = $"{Guid.NewGuid():N}@example.com", IsPrimary = true };
        await repository.AddAsync(contact, CancellationToken.None); await context.SaveChangesAsync(CancellationToken.None); context.ChangeTracker.Clear();
        var result = await repository.GetByIdAsync(supplier.ID, contact.ID, CancellationToken.None);
        Assert.NotNull(result); Assert.Equal("Buyer", result.Name); Assert.Equal(8, result.RowVersion.Length);
        Assert.Null(await repository.GetByIdAsync(supplier.ID + 1, contact.ID, CancellationToken.None));
    }

    [Fact]
    public async Task SupplierContactRepository_DuplicatePrimary_DatabaseRejectsSecondContact()
    {
        await using var context = _fixture.CreateContext(); await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var supplier = await AddSupplierAsync(context); context.SupplierContacts.Add(new SupplierContact { SupplierID = supplier.ID, Name = "One", IsPrimary = true }); await context.SaveChangesAsync(CancellationToken.None);
        context.SupplierContacts.Add(new SupplierContact { SupplierID = supplier.ID, Name = "Two", IsPrimary = true });
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task SupplierContactRepository_EmailQueriesRespectSupplierAndContactOwnership()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var supplier = await AddSupplierAsync(context);
        var repository = new SupplierContactRepository(context);
        var contact = new SupplierContact { SupplierID = supplier.ID, Name = "Buyer", Email = $"buyer-{Guid.NewGuid():N}@example.com" };
        await repository.AddAsync(contact, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        Assert.True(await repository.EmailExistsForSupplierAsync(supplier.ID, contact.Email!, CancellationToken.None));
        Assert.False(await repository.EmailExistsForOtherContactAsync(supplier.ID, contact.ID, contact.Email!, CancellationToken.None));
        Assert.False(await repository.EmailExistsForSupplierAsync(supplier.ID + 1, contact.Email!, CancellationToken.None));
    }

    [Fact]
    public async Task SupplierAddressRepository_PrimaryQueryFiltersByType()
    {
        await using var context = _fixture.CreateContext(); await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var supplier = await AddSupplierAsync(context); var repository = new SupplierAddressRepository(context);
        await repository.AddAsync(Address(supplier.ID, SupplierAddressType.Billing, true), CancellationToken.None);
        await repository.AddAsync(Address(supplier.ID, SupplierAddressType.Ordering, true), CancellationToken.None); await context.SaveChangesAsync(CancellationToken.None); context.ChangeTracker.Clear();
        var billing = await repository.GetPrimaryByTypeAsync(supplier.ID, SupplierAddressType.Billing, CancellationToken.None);
        Assert.NotNull(billing); Assert.Equal(SupplierAddressType.Billing, billing.Type); Assert.Equal(8, billing.RowVersion.Length);
    }

    [Fact]
    public async Task SupplierAddressRepository_DuplicatePrimaryForType_DatabaseRejectsSecondAddress()
    {
        await using var context = _fixture.CreateContext(); await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var supplier = await AddSupplierAsync(context); context.SupplierAddresses.Add(Address(supplier.ID, SupplierAddressType.Billing, true)); await context.SaveChangesAsync(CancellationToken.None);
        context.SupplierAddresses.Add(Address(supplier.ID, SupplierAddressType.Billing, true));
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task SupplierAddressRepository_OwnedAndTypeQueriesReturnOnlyMatchingAddresses()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var supplier = await AddSupplierAsync(context);
        var repository = new SupplierAddressRepository(context);
        var billing = Address(supplier.ID, SupplierAddressType.Billing, false);
        var ordering = Address(supplier.ID, SupplierAddressType.Ordering, false);
        await repository.AddAsync(billing, CancellationToken.None);
        await repository.AddAsync(ordering, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);
        context.ChangeTracker.Clear();

        var billingResults = (await repository.GetByTypeAsync(supplier.ID, SupplierAddressType.Billing, CancellationToken.None)).ToList();

        Assert.Single(billingResults);
        Assert.Equal(billing.ID, billingResults[0].ID);
        Assert.Null(await repository.GetByIdAsync(supplier.ID + 1, billing.ID, CancellationToken.None));
        Assert.True(await repository.ExistsAsync(supplier.ID, ordering.ID, CancellationToken.None));
    }

    [Fact]
    public async Task SupplierProductRepository_CompositeLookupIncludesRelationships()
    {
        await using var context = _fixture.CreateContext(); await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var supplier = await AddSupplierAsync(context); var product = await AddProductAsync(context); var repository = new SupplierProductRepository(context);
        await repository.AddAsync(new SupplierProduct { SupplierID = supplier.ID, ProductID = product.ID, SupplierSku = $"SKU-{Guid.NewGuid():N}", UnitCost = 8, MinimumOrderQuantity = 1, IsPreferred = true }, CancellationToken.None); await context.SaveChangesAsync(CancellationToken.None); context.ChangeTracker.Clear();
        var result = await repository.GetAsync(supplier.ID, product.ID, CancellationToken.None);
        Assert.NotNull(result); Assert.Equal(supplier.Name, result.Supplier.Name); Assert.Equal(product.Name, result.Product.Name); Assert.Equal(8, result.RowVersion.Length);
    }

    [Fact]
    public async Task SupplierProductRepository_SecondPreferredSupplier_DatabaseRejectsAssignment()
    {
        await using var context = _fixture.CreateContext(); await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var supplier1 = await AddSupplierAsync(context); var supplier2 = await AddSupplierAsync(context); var product = await AddProductAsync(context);
        context.SupplierProducts.Add(new SupplierProduct { SupplierID = supplier1.ID, ProductID = product.ID, MinimumOrderQuantity = 1, IsPreferred = true }); await context.SaveChangesAsync(CancellationToken.None);
        context.SupplierProducts.Add(new SupplierProduct { SupplierID = supplier2.ID, ProductID = product.ID, MinimumOrderQuantity = 1, IsPreferred = true });
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task SupplierProductRepository_AssignmentAndSkuQueriesFindExistingAssignment()
    {
        await using var context = _fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);
        var supplier = await AddSupplierAsync(context);
        var product = await AddProductAsync(context);
        var repository = new SupplierProductRepository(context);
        var supplierSku = $"SKU-{Guid.NewGuid():N}";
        await repository.AddAsync(new SupplierProduct
        {
            SupplierID = supplier.ID, ProductID = product.ID, SupplierSku = supplierSku,
            UnitCost = 5, MinimumOrderQuantity = 1
        }, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        Assert.True(await repository.AssignmentExistsAsync(supplier.ID, product.ID, CancellationToken.None));
        Assert.True(await repository.SupplierSkuExistsAsync(supplier.ID, supplierSku, CancellationToken.None));
        Assert.False(await repository.SupplierSkuExistsForOtherProductAsync(supplier.ID, product.ID, supplierSku, CancellationToken.None));
    }

    private static async Task<Supplier> AddSupplierAsync(Services.InvManDBContext context)
    {
        var supplier = new Supplier { Name = $"Supplier-{Guid.NewGuid():N}", IsActive = true }; context.Suppliers.Add(supplier); await context.SaveChangesAsync(CancellationToken.None); return supplier;
    }

    private static async Task<Product> AddProductAsync(Services.InvManDBContext context)
    {
        var category = new Category { Name = $"Category-{Guid.NewGuid():N}", Description = "Test", IsActive = true }; context.Categories.Add(category); await context.SaveChangesAsync(CancellationToken.None);
        var product = new Product { Sku = $"P-{Guid.NewGuid():N}", Name = $"Product-{Guid.NewGuid():N}", Description = "Test", CategoryID = category.ID, Price = 10, IsActive = true }; context.Products.Add(product); await context.SaveChangesAsync(CancellationToken.None); return product;
    }

    private static SupplierAddress Address(int supplierId, SupplierAddressType type, bool primary) => new() { SupplierID = supplierId, Type = type, AddressLine1 = "1 Test Street", City = "Brisbane", PostalCode = "4000", CountryCode = "AU", IsPrimary = primary, IsActive = true };
}
