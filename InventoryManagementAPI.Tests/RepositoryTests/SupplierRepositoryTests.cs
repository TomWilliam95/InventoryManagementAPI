using InventoryManagementAPI.Models.CoreModels.SupplierModels;
using InventoryManagementAPI.Repositories.SupplierRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Tests.RepositoryTests
{
    public class SupplierRepositoryTests : IClassFixture<SqlServerFixture>
    {
        private readonly SqlServerFixture _fixture;
        public SupplierRepositoryTests(SqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        // GET BY ID
        [Fact]
        public async Task GetSupplierByIdAsync_ExistingSupplier_ReturnsSupplier()
        {
            // Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var testSupplier = CreateSupplier();
            context.Suppliers.Add(testSupplier);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var supplierRepository = new SupplierRepository(context);
            // Act
            var retrievedSupplier = await supplierRepository.GetSupplierByIdAsync(testSupplier.ID, CancellationToken.None);

            // Assert
            Assert.NotNull(retrievedSupplier);
            Assert.True(retrievedSupplier.ID > 0);
            Assert.Equal("Test Supplier", retrievedSupplier.Name);
        }
        [Fact]
        public async Task GetSupplierByIdAsync_MissingSupplier_ReturnsNull()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var supplierRepository = new SupplierRepository(context);
            // Act
            var retrievedSupplier = await supplierRepository.GetSupplierByIdAsync(int.MaxValue, CancellationToken.None);

            // Assert
            Assert.Null(retrievedSupplier);
        }

        // GET ALL SUPPLIERS
        [Fact]
        public async Task GetAllSuppliersAsync_SupplierExists_ReturnsSupplier()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction1 = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var testSupplier = CreateSupplier();
            testSupplier.Name = $"Get-all supplier {Guid.NewGuid():N}";
            await context.Suppliers.AddAsync(testSupplier, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var repository = new SupplierRepository(context);
            //Act
            var retrievedSuppliers = (await repository.GetAllSuppliersAsync(CancellationToken.None)).ToList();

            //Assert
            Assert.Contains(retrievedSuppliers, s =>
                s.ID == testSupplier.ID &&
                s.Name == testSupplier.Name);
        }

        //Create Supplier Test
        [Fact]
        public async Task CreateSupplierAsync_ValidSupplier_PersistsSupplier()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var testSupplier = CreateSupplier();

            var supplierRepository = new SupplierRepository(context);

            //Act
            var createdSupplier = await supplierRepository.CreateSupplierAsync(testSupplier, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            var supplierId = createdSupplier.ID;
            context.ChangeTracker.Clear();

            //Assert
            var persistedSupplier = await context.Suppliers.FindAsync([supplierId], CancellationToken.None);
            Assert.NotNull(persistedSupplier);
            Assert.Equal(testSupplier.Name, persistedSupplier.Name);
        }

        //Update Supplier Test
        [Fact]
        public async Task UpdateSupplierAsync_ValidChanges_PersistsUpdatedSupplier()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            //Create test Supplier
            var testSupplier = CreateSupplier();
            await context.Suppliers.AddAsync(testSupplier, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var supplierRepository = new SupplierRepository(context);

            //Act
            var supplierToUpdate = await context.Suppliers.FindAsync([testSupplier.ID], CancellationToken.None);
            Assert.NotNull(supplierToUpdate);
            supplierToUpdate.Name = "Updated Supplier Name";
            supplierToUpdate.Website = "https://updated.example.com";

            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var supplierResult = await context.Suppliers.FindAsync([testSupplier.ID], CancellationToken.None);

            //Assert
            Assert.NotNull(supplierResult);
            Assert.Equal("Updated Supplier Name", supplierResult.Name);
            Assert.Equal("https://updated.example.com", supplierResult.Website);
        }

        //SupplierExists Test
        [Fact]
        public async Task SupplierExistsAsync_ExistingSupplier_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var testSupplier = CreateSupplier();
            await context.Suppliers.AddAsync(testSupplier, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var supplierRepository = new SupplierRepository(context);

            //Act
            var exists = await supplierRepository.SupplierExistsAsync(testSupplier.ID, CancellationToken.None);

            //Assert
            Assert.True(exists);
        }
        [Fact]
        public async Task SupplierExistsAsync_MissingSupplier_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var supplierRepository = new SupplierRepository(context);

            //Act
            var exists = await supplierRepository.SupplierExistsAsync(int.MaxValue, CancellationToken.None);

            //Assert
            Assert.False(exists);
        }

        //SupplierNameExists Test
        [Fact]
        public async Task SupplierNameExistsAsync_Match_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var testSupplier = CreateSupplier();
            testSupplier.Name = "Unique Supplier Name";
            await context.Suppliers.AddAsync(testSupplier, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();


            var supplierRepository = new SupplierRepository(context);

            //Act
            var nameExists = await supplierRepository.SupplierNameExistsAsync("Unique Supplier Name", CancellationToken.None);

            //Assert
            Assert.True(nameExists);
        }
        [Fact]
        public async Task SupplierNameExistsAsync_NoMatch_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var supplierRepository = new SupplierRepository(context);
            //Act
            var nameExists = await supplierRepository.SupplierNameExistsAsync("NonExistent Supplier Name", CancellationToken.None);

            //Assert
            Assert.False(nameExists);
        }

        //SupplierNameExistsForOther Test
        [Fact]
        public async Task OtherSupplierNameExistsAsync_Match_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var existingSupplier = CreateSupplier();
            existingSupplier.Name = $"Duplicate supplier {Guid.NewGuid():N}";
            existingSupplier.TaxNumber = $"TAX-{Guid.NewGuid():N}";
            var testSupplier = CreateSupplier();
            testSupplier.Name = $"Current supplier {Guid.NewGuid():N}";
            testSupplier.TaxNumber = $"TAX-{Guid.NewGuid():N}";
            await context.AddRangeAsync([existingSupplier, testSupplier], CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();
            
            var supplierRepository = new SupplierRepository(context);

            //Act
            var matchResult = await supplierRepository.SupplierNameExistsForOtherSupplierAsync(testSupplier.ID, existingSupplier.Name, CancellationToken.None);
            //Assert
            Assert.True(matchResult);
        }
        [Fact]
        public async Task OtherSupplierNameExistsAsync_NoMatch_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken.None);

            var testSupplier = CreateSupplier();
            testSupplier.Name = "Unique SupplierName";
            await context.AddAsync(testSupplier, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
            context.ChangeTracker.Clear();

            var supplierRepository = new SupplierRepository(context);

            //Act
            var matchResult = await supplierRepository.SupplierNameExistsForOtherSupplierAsync(testSupplier.ID, "Unique SupplierName", CancellationToken.None);
            //Assert
            Assert.False(matchResult);
        }

        //Helper Method
        private static Supplier CreateSupplier()
        {
            return new Supplier
            {
                Name = "Test Supplier",
                TaxNumber = "TAX-001",
                Website = "https://supplier.example.com",
                IsActive = true,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
        }
    }
}
