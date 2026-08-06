using InventoryManagementAPI.Models.CoreModels;
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
            await using var transaction = await context.Database.BeginTransactionAsync();

            var testSupplier = CreateSupplier();
            context.Suppliers.Add(testSupplier);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var supplierRepository = new SupplierRepository(context);
            // Act
            var retrievedSupplier = await supplierRepository.GetSupplierByIdAsync(testSupplier.ID);

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
            await using var transaction = await context.Database.BeginTransactionAsync();

            var supplierRepository = new SupplierRepository(context);
            // Act
            var retrievedSupplier = await supplierRepository.GetSupplierByIdAsync(int.MaxValue);

            // Assert
            Assert.Null(retrievedSupplier);
        }

        // GET ALL SUPPLIERS
        [Fact]
        public async Task GetAllSuppliersAsync_SupplierExists_ReturnsSupplier()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction1 = await context.Database.BeginTransactionAsync();

            var testSupplier = CreateSupplier();
            testSupplier.Name = $"Get-all supplier {Guid.NewGuid():N}";
            await context.Suppliers.AddAsync(testSupplier);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new SupplierRepository(context);
            //Act
            var retrievedSuppliers = (await repository.GetAllSuppliersAsync()).ToList();

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
            await using var transaction = await context.Database.BeginTransactionAsync();

            var testSupplier = CreateSupplier();

            var supplierRepository = new SupplierRepository(context);

            //Act
            var createdSupplier = await supplierRepository.CreateSupplierAsync(testSupplier);
            await supplierRepository.SaveChangesAsync();
            var supplierId = createdSupplier.ID;
            context.ChangeTracker.Clear();

            //Assert
            var persistedSupplier = await context.Suppliers.FindAsync(supplierId);
            Assert.NotNull(persistedSupplier);
            Assert.Equal(testSupplier.Name, persistedSupplier.Name);
        }

        //Update Supplier Test
        [Fact]
        public async Task UpdateSupplierAsync_ValidChanges_PersistsUpdatedSupplier()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            //Create test Supplier
            var testSupplier = CreateSupplier();
            await context.Suppliers.AddAsync(testSupplier);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var supplierRepository = new SupplierRepository(context);

            //Act
            var supplierToUpdate = await context.Suppliers.FindAsync(testSupplier.ID);
            Assert.NotNull(supplierToUpdate);
            supplierToUpdate.Name = "Updated Supplier Name";
            supplierToUpdate.EmailContact = "UpdatedEmail@example.com";

            await supplierRepository.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var supplierResult = await context.Suppliers.FindAsync(testSupplier.ID);

            //Assert
            Assert.NotNull(supplierResult);
            Assert.Equal("Updated Supplier Name", supplierResult.Name);
            Assert.Equal("UpdatedEmail@example.com", supplierResult.EmailContact);
        }

        //SupplierExists Test
        [Fact]
        public async Task SupplierExistsAsync_ExistingSupplier_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var testSupplier = CreateSupplier();
            await context.Suppliers.AddAsync(testSupplier);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var supplierRepository = new SupplierRepository(context);

            //Act
            var exists = await supplierRepository.SupplierExistsAsync(testSupplier.ID);

            //Assert
            Assert.True(exists);
        }
        [Fact]
        public async Task SupplierExistsAsync_MissingSupplier_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var supplierRepository = new SupplierRepository(context);

            //Act
            var exists = await supplierRepository.SupplierExistsAsync(int.MaxValue);

            //Assert
            Assert.False(exists);
        }

        //SupplierNameExists Test
        [Fact]
        public async Task SupplierNameExistsAsync_Match_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var testSupplier = CreateSupplier();
            testSupplier.Name = "Unique Supplier Name";
            await context.Suppliers.AddAsync(testSupplier);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();


            var supplierRepository = new SupplierRepository(context);

            //Act
            var nameExists = await supplierRepository.SupplierNameExistsAsync("Unique Supplier Name");

            //Assert
            Assert.True(nameExists);
        }
        [Fact]
        public async Task SupplierNameExistsAsync_NoMatch_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var supplierRepository = new SupplierRepository(context);
            //Act
            var nameExists = await supplierRepository.SupplierNameExistsAsync("NonExistent Supplier Name");

            //Assert
            Assert.False(nameExists);
        }

        //SupplierNameExistsForOther Test
        [Fact]
        public async Task OtherSupplierNameExistsAsync_Match_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var existingSupplier = CreateSupplier();
            existingSupplier.Name = $"Duplicate supplier {Guid.NewGuid():N}";
            var testSupplier = CreateSupplier();
            testSupplier.Name = existingSupplier.Name;
            await context.AddRangeAsync(existingSupplier, testSupplier);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            
            var supplierRepository = new SupplierRepository(context);

            //Act
            var matchResult = await supplierRepository.SupplierNameExistsForOtherSupplierAsync(testSupplier.ID, testSupplier.Name);
            //Assert
            Assert.True(matchResult);
        }
        [Fact]
        public async Task OtherSupplierNameExistsAsync_NoMatch_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var testSupplier = CreateSupplier();
            testSupplier.Name = "Unique SupplierName";
            await context.AddAsync(testSupplier);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var supplierRepository = new SupplierRepository(context);

            //Act
            var matchResult = await supplierRepository.SupplierNameExistsForOtherSupplierAsync(testSupplier.ID, "Unique SupplierName");
            //Assert
            Assert.False(matchResult);
        }

        //SupplierEmail Exists Test
        [Fact]
        public async Task SupplierEmailExistsAsync_Match_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var testSupplier = CreateSupplier();
            testSupplier.EmailContact = $"email-{Guid.NewGuid():N}@example.com";
            await context.Suppliers.AddAsync(testSupplier);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var supplierRepository = new SupplierRepository(context);
            
            //Act
            var emailExists = await supplierRepository.SupplierEmailExistsAsync(testSupplier.EmailContact);

            //Assert
            Assert.True(emailExists);
        }
        [Fact]
        public async Task SupplierEmailExistsAsync_NoMatch_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var supplierRepository = new SupplierRepository(context);

            //Act
            var emailExists = await supplierRepository.SupplierEmailExistsAsync("Unique Email");

            //Assert
            Assert.False(emailExists);
        }

        //OtherSupplierEmail Exists Test
        [Fact]
        public async Task OtherSupplierEmailExistsAsync_Match_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();
            var existingSupplier = CreateSupplier();
            existingSupplier.EmailContact = $"shared-{Guid.NewGuid():N}@example.com";
            var testSupplier = CreateSupplier();
            testSupplier.EmailContact = existingSupplier.EmailContact;
            await context.AddRangeAsync(existingSupplier, testSupplier);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new SupplierRepository(context);
            //Act
            var matchResult = await repository.SupplierEmailExistsForOtherSupplierAsync(testSupplier.ID, testSupplier.EmailContact);
            //Assert
            Assert.True(matchResult);
        }
        [Fact]
        public async Task OtherSupplierEmailExistsAsync_NoMatch_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();
            var testSupplier = CreateSupplier();
            testSupplier.EmailContact = "Unique Email";
            await context.AddAsync(testSupplier);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var supplierRepository = new SupplierRepository(context);

            //Act
            var emailExists = await supplierRepository.SupplierEmailExistsForOtherSupplierAsync(testSupplier.ID, "Unique Email");

            //Assert
            Assert.False(emailExists);
        }


        //Helper Method
        private static Supplier CreateSupplier()
        {
            return new Supplier
            {
                Name = "Test Supplier",
                ContactName = "John Doe",
                PhoneContact = "123-456-7890",
                EmailContact = "johndoe@example.com",
                Address = "123 Test St, Test City, TS 12345",
                IsActive = true,
                Created = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}
