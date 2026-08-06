using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Tests.RepositoryTests;

public class EntityConcurrencyTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;

    public EntityConcurrencyTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Category_StaleUpdate_ThrowsConcurrencyExceptionAndPreservesWinningUpdate()
    {
        int categoryId;
        byte[] originalRowVersion;

        await using (var setupContext = _fixture.CreateContext())
        {
            var category = new Category
            {
                Name = $"Concurrency category {Guid.NewGuid():N}",
                Description = "Original category description",
                IsActive = true,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            setupContext.Categories.Add(category);
            await setupContext.SaveChangesAsync();

            categoryId = category.ID;
            originalRowVersion = category.RowVersion.ToArray();
        }

        await using var contextA = _fixture.CreateContext();
        await using var contextB = _fixture.CreateContext();

        var categoryA = await contextA.Categories.FindAsync(categoryId);
        var categoryB = await contextB.Categories.FindAsync(categoryId);

        Assert.NotNull(categoryA);
        Assert.NotNull(categoryB);
        Assert.NotEmpty(originalRowVersion);
        Assert.Equal(originalRowVersion, categoryA.RowVersion);
        Assert.Equal(originalRowVersion, categoryB.RowVersion);

        categoryA.Description = "Winning category description";
        await contextA.SaveChangesAsync();

        categoryB.Description = "Stale category description";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => contextB.SaveChangesAsync());

        await using var verifyContext = _fixture.CreateContext();
        var storedCategory = await verifyContext.Categories
            .AsNoTracking()
            .SingleAsync(category => category.ID == categoryId);

        Assert.Equal("Winning category description", storedCategory.Description);
        Assert.Equal(categoryA.RowVersion, storedCategory.RowVersion);
        Assert.NotEqual(originalRowVersion, storedCategory.RowVersion);
    }

    [Fact]
    public async Task Supplier_StaleUpdate_ThrowsConcurrencyExceptionAndPreservesWinningUpdate()
    {
        int supplierId;
        byte[] originalRowVersion;

        await using (var setupContext = _fixture.CreateContext())
        {
            var supplier = new Supplier
            {
                Name = $"Concurrency supplier {Guid.NewGuid():N}",
                ContactName = "Original Contact",
                PhoneContact = "0400000000",
                EmailContact = $"{Guid.NewGuid():N}@example.com",
                Address = "Original supplier address",
                IsActive = true,
                Created = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            setupContext.Suppliers.Add(supplier);
            await setupContext.SaveChangesAsync();

            supplierId = supplier.ID;
            originalRowVersion = supplier.RowVersion.ToArray();
        }

        await using var contextA = _fixture.CreateContext();
        await using var contextB = _fixture.CreateContext();

        var supplierA = await contextA.Suppliers.FindAsync(supplierId);
        var supplierB = await contextB.Suppliers.FindAsync(supplierId);

        Assert.NotNull(supplierA);
        Assert.NotNull(supplierB);
        Assert.NotEmpty(originalRowVersion);
        Assert.Equal(originalRowVersion, supplierA.RowVersion);
        Assert.Equal(originalRowVersion, supplierB.RowVersion);

        supplierA.ContactName = "Winning Contact";
        await contextA.SaveChangesAsync();

        supplierB.ContactName = "Stale Contact";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => contextB.SaveChangesAsync());

        await using var verifyContext = _fixture.CreateContext();
        var storedSupplier = await verifyContext.Suppliers
            .AsNoTracking()
            .SingleAsync(supplier => supplier.ID == supplierId);

        Assert.Equal("Winning Contact", storedSupplier.ContactName);
        Assert.Equal(supplierA.RowVersion, storedSupplier.RowVersion);
        Assert.NotEqual(originalRowVersion, storedSupplier.RowVersion);
    }

    [Fact]
    public async Task User_StaleUpdate_ThrowsConcurrencyExceptionAndPreservesWinningUpdate()
    {
        int userId;
        byte[] originalRowVersion;

        await using (var setupContext = _fixture.CreateContext())
        {
            var user = new User
            {
                UserName = $"ConcurrencyUser{Guid.NewGuid():N}"[..30],
                Email = $"{Guid.NewGuid():N}@example.com",
                Password_Hash = BCrypt.Net.BCrypt.EnhancedHashPassword("Password1!"),
                Role = UserRoles.Staff,
                IsActive = true,
                Created = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };

            setupContext.Users.Add(user);
            await setupContext.SaveChangesAsync();

            userId = user.ID;
            originalRowVersion = user.RowVersion.ToArray();
        }

        await using var contextA = _fixture.CreateContext();
        await using var contextB = _fixture.CreateContext();

        var userA = await contextA.Users.FindAsync(userId);
        var userB = await contextB.Users.FindAsync(userId);

        Assert.NotNull(userA);
        Assert.NotNull(userB);
        Assert.NotEmpty(originalRowVersion);
        Assert.Equal(originalRowVersion, userA.RowVersion);
        Assert.Equal(originalRowVersion, userB.RowVersion);

        userA.UserName = "WinningUserName";
        await contextA.SaveChangesAsync();

        userB.UserName = "StaleUserName";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => contextB.SaveChangesAsync());

        await using var verifyContext = _fixture.CreateContext();
        var storedUser = await verifyContext.Users
            .AsNoTracking()
            .SingleAsync(user => user.ID == userId);

        Assert.Equal("WinningUserName", storedUser.UserName);
        Assert.Equal(userA.RowVersion, storedUser.RowVersion);
        Assert.NotEqual(originalRowVersion, storedUser.RowVersion);
    }
}
