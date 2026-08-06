using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Repositorys.ProductRepositories;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;


namespace InventoryManagementAPI.Tests.RepositoryTests
{
    public class ProductRepositoryTests : IClassFixture<SqlServerFixture>
    {
        private readonly SqlServerFixture _fixture;

        public ProductRepositoryTests(SqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        //GetProductById
        [Fact]
        public async Task GetProductAsync_ExistingProduct_ReturnsProduct()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductAsync(context);
            context.Products.Add(testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new ProductRepository(context);

            //Act
            var result = await repository.GetProductAsync(testProduct.ID);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(testProduct.ID, result.ID);
            Assert.Equal(testProduct.Name, result.Name);
            Assert.NotNull(result.Category);
            Assert.NotNull(result.Supplier);
        }
        [Fact]
        public async Task GetProductAsync_MissingProduct_ReturnsNull()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var repository = new ProductRepository(context);

            //Act
            var result = await repository.GetProductAsync(int.MaxValue);
            //Assert
            Assert.Null(result);
        }

        //GetAllProducts
        [Fact]
        public async Task GetAllProductsAsync_ProductExists_ReturnsProduct()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductAsync(context);
            await context.Products.AddAsync(testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new ProductRepository(context);

            //Act
            var result = (await repository.GetAllProductsAsync()).ToList();

            //Assert
            Assert.Contains(result, p =>
                p.ID == testProduct.ID &&
                p.Name == testProduct.Name);
            var retrievedProduct = Assert.Single(result, p => p.ID == testProduct.ID);
            Assert.NotNull(retrievedProduct.Category);
            Assert.NotNull(retrievedProduct.Supplier);
        }

        //GetProductsByCategory
        [Fact]
        public async Task GetProductsByCategoryAsync_MatchingProducts_ReturnsProducts()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var category = await CreateCategoryAsync(context);
            var supplier = await CreateSupplierAsync(context);
            var firstProduct = CreateTestProduct(category.ID, supplier.ID);
            var secondProduct = CreateTestProduct(category.ID, supplier.ID);
            await context.Products.AddRangeAsync(firstProduct, secondProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new ProductRepository(context);
            //Act
            var result = (await repository.GetProductsByCategoryAsync(category.ID)).ToList();

            //Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, product => product.ID == firstProduct.ID);
            Assert.Contains(result, product => product.ID == secondProduct.ID);
            Assert.All(result, product =>
            {
                Assert.NotNull(product.Category);
                Assert.NotNull(product.Supplier);
            });
        }
        [Fact]
        public async Task GetProductsByCategoryAsync_NoMatchingProducts_ReturnsEmpty()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var categoryWithProduct = await CreateCategoryAsync(context);
            var categoryWithoutProducts = await CreateCategoryAsync(context);
            var supplier = await CreateSupplierAsync(context);
            var product = CreateTestProduct(categoryWithProduct.ID, supplier.ID);
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new ProductRepository(context);
            //Act
            var result = await repository.GetProductsByCategoryAsync(categoryWithoutProducts.ID);

            //Assert
            Assert.Empty(result);
        }

        // Get products below reorder level
        [Fact]
        public async Task GetProductsBelowReorderLevel_ProductBelowLevel_ReturnsProduct()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductAsync(context); //Create a product with a quantity below the reorder level
            testProduct.ReorderLevel = 100;
            testProduct.QuantityInStock = 99;
            await context.Products.AddAsync(testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new ProductRepository(context);

            //Act
            var result = await repository.GetProductsBelowReorderLevelAsync();

            //Assert
            Assert.Contains(result,p => p.QuantityInStock == 99);
        }
        [Fact]
        public async Task GetProductsBelowReorderLevel_NoProductsBelowLevel_ReturnsEmpty()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductAsync(context);
            testProduct.QuantityInStock = 25;
            testProduct.ReorderLevel = 10;
            await context.Products.AddAsync(testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new ProductRepository(context);

            //Act
            var result = await repository.GetProductsBelowReorderLevelAsync();

            //Assert
            Assert.Empty(result);
        }

        //AddProduct
        [Fact]
        public async Task AddProductAsync_ValidProduct_PersistsProduct()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            
            var testProduct = await CreateTestProductAsync(context);

            var repository = new ProductRepository(context);

            //Act
            var result = await repository.AddProductAsync(testProduct);
            await context.SaveChangesAsync();

            //Assert
            context.ChangeTracker.Clear();
            var persistedProduct = await context.Products.FindAsync(result.ID);
            Assert.NotNull(persistedProduct);
            Assert.Equal(testProduct.Name, persistedProduct.Name);
        }

        //ChecksProductExists
        [Fact]
        public async Task ProductExistsAsync_ExistingProduct_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductAsync(context);
            await context.Products.AddAsync(testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new ProductRepository(context);
            //Act
            var result = await repository.ProductExistsAsync(testProduct.ID);
            //Assert
            Assert.True(result);
        }
        [Fact]
        public async Task ProductExistsAsync_NoMatch_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            var repository = new ProductRepository(context);
            //Act
            var result = await repository.ProductExistsAsync(int.MaxValue);
            //Assert
            Assert.False(result);
        }

        //CheckProductNameExists
        [Fact]
        public async Task ProductNameExistsAsync_Match_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductAsync(context);
            await context.Products.AddAsync(testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new ProductRepository(context);
            //Act
            var result = await repository.ProductNameExistsAsync(testProduct.Name);
            //Assert
            Assert.True(result);
        }
        [Fact]
        public async Task ProductNameExistsAsync_NoMatch_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            var repository = new ProductRepository(context);
            //Act
            var result = await repository.ProductNameExistsAsync($"Missing product {Guid.NewGuid():N}");
            //Assert
            Assert.False(result);
        }
        [Fact]
        public async Task OtherProductNameExistsAsync_Match_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            
            var existingProduct = await CreateTestProductAsync(context);
            existingProduct.Name = $"Shared product {Guid.NewGuid():N}";
            var testProduct = CreateTestProduct(existingProduct.CategoryID, existingProduct.SupplierID);
            testProduct.Name = existingProduct.Name;
            await context.AddRangeAsync(existingProduct, testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new ProductRepository(context);
            //Act
            var result = await repository.OtherProductNameExistsAsync(testProduct.ID, testProduct.Name);
            //Assert
            Assert.True(result);
        }
        [Fact]
        public async Task OtherProductNameExistsAsync_NoMatch_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            
            var testProduct = await CreateTestProductAsync(context);
            testProduct.Name = $"Unique product {Guid.NewGuid():N}";
            await context.AddAsync(testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new ProductRepository(context);
            //Act
            var result = await repository.OtherProductNameExistsAsync(testProduct.ID, testProduct.Name);
            //Assert
            Assert.False(result);
        }

        //CheckProductSkuExists
        [Fact]
        public async Task ProductSkuExistsAsync_Match_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductAsync(context);
            testProduct.Sku = $"SKU{Guid.NewGuid():N}"[..8];
            await context.Products.AddAsync(testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new ProductRepository(context);
            //Act
            var result = await repository.ProductSkuExistsAsync(testProduct.Sku);
            //Assert
            Assert.True(result);
        }
        [Fact]
        public async Task ProductSkuExistsAsync_NoMatch_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            var repository = new ProductRepository(context);
            //Act
            var result = await repository.ProductSkuExistsAsync($"SKU{Guid.NewGuid():N}"[..8]);
            //Assert
            Assert.False(result);
        }
        [Fact]
        public async Task OtherProductSkuExistsAsync_Match_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            
            var existingProduct = await CreateTestProductAsync(context);
            existingProduct.Sku = $"SKU{Guid.NewGuid():N}"[..8];
            var testProduct = CreateTestProduct(existingProduct.CategoryID, existingProduct.SupplierID);
            testProduct.Sku = existingProduct.Sku;
            await context.AddRangeAsync(existingProduct, testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new ProductRepository(context);
            //Act
            var result = await repository.OtherProductSkuExistsAsync(testProduct.ID, testProduct.Sku);
            //Assert
            Assert.True(result);
        }
        [Fact]
        public async Task OtherProductSkuExistsAsync_NoMatch_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            
            var testProduct = await CreateTestProductAsync(context);
            testProduct.Sku = $"SKU{Guid.NewGuid():N}"[..8];
            await context.AddAsync(testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var repository = new ProductRepository(context);
            //Act
            var result = await repository.OtherProductSkuExistsAsync(testProduct.ID, testProduct.Sku);
            //Assert
            Assert.False(result);
        }

        //SaveChanges Tests
        [Fact]
        public async Task SaveChangesAsync_TrackedProductChanged_PersistsUpdatedProduct()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            var testProduct = await CreateTestProductAsync(context);
            await context.Products.AddAsync(testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var repository = new ProductRepository(context);

            //Act
            var updateTestProduct = await context.Products.FindAsync(testProduct.ID);
            Assert.NotNull(updateTestProduct);
            updateTestProduct.Name = $"Updated product {Guid.NewGuid():N}";
            await repository.SaveChangesAsync();

            //Assert
            context.ChangeTracker.Clear();
            var persistedProduct = await context.Products.FindAsync(testProduct.ID);
            Assert.NotNull(persistedProduct);
            Assert.Equal(updateTestProduct.Name, persistedProduct.Name);
        }

        // Is-active tests
        [Fact]
        public async Task IsProductActiveAsync_CheckActiveProduct_ReturnsTrue()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            var testProduct = await CreateTestProductAsync(context);
            testProduct.IsActive = true;
            await context.Products.AddAsync(testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var repository = new ProductRepository(context);
            //Act
            var result = await repository.IsProductActiveAsync(testProduct.ID);
            //Assert
            Assert.True(result);
        }
        [Fact]
        public async Task IsProductActiveAsync_CheckInactiveProduct_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            var testProduct = await CreateTestProductAsync(context);
            testProduct.IsActive = false;
            await context.Products.AddAsync(testProduct);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var repository = new ProductRepository(context);
            //Act
            var result = await repository.IsProductActiveAsync(testProduct.ID);
            //Assert
            Assert.False(result);
        }
        [Fact]
        public async Task IsProductActiveAsync_MissingProduct_ReturnsFalse()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();
            var repository = new ProductRepository(context);
            //Act
            var result = await repository.IsProductActiveAsync(int.MaxValue);
            //Assert
            Assert.False(result);
        }

        //Concurrency Tests
        [Fact]
        public async Task ConcurrencyTests_UpdatingProduct_ReturnDifferentRowVersionAfterUpdate()
        {
            //Arrange
            await using var context = _fixture.CreateContext();
            await using var transaction = context.Database.BeginTransaction();

            var testProduct = await CreateTestProductAsync(context);
            await context.Products.AddAsync(testProduct);
            await context.SaveChangesAsync();
            var rowVersionBeforeUpdate = testProduct.RowVersion; // Store the RowVersion before the update

            context.ChangeTracker.Clear();

            //Act
            testProduct = await context.Products.FindAsync(testProduct.ID);
            Assert.NotNull(testProduct);
            testProduct.Name = "Updated product name for concurrency test"; // Update the product's name, to trigger a change in RowVersion
            await context.SaveChangesAsync();
            var rowVersionAfterUpdate = testProduct.RowVersion; // Store the RowVersion after the update

            //Assert
            Assert.NotEmpty(rowVersionBeforeUpdate);
            Assert.NotEmpty(rowVersionAfterUpdate);
            Assert.NotEqual(rowVersionBeforeUpdate, rowVersionAfterUpdate); //Compares RowVersions ensuring Update occured, and RowVersions differ
        }
        [Fact]
        public async Task ConcurrencyTests_SavingStaleProduct_RefuseUpdateThrowConcurrencyException()
        {
            //Arrange
            await using var setupContext = _fixture.CreateContext(); // Create a separate context for setup to avoid tracking issues

            var testProduct = await CreateTestProductAsync(setupContext);
            await setupContext.Products.AddAsync(testProduct);
            await setupContext.SaveChangesAsync();

            int productId = testProduct.ID;
            int supplierId = testProduct.SupplierID;
            int categoryId = testProduct.CategoryID;

            var originalRowVersion = testProduct.RowVersion; // Store the original RowVersion for later comparison
            setupContext.ChangeTracker.Clear();

            await using var contextA = _fixture.CreateContext();
            var productA = await contextA.Products.FindAsync(productId);
            Assert.NotNull(productA);
            Assert.Equal(originalRowVersion, productA.RowVersion); // Ensure the RowVersion matches the original

            await using var contextB = _fixture.CreateContext();
            var productB = await contextB.Products.FindAsync(productId);
            Assert.NotNull(productB);
            Assert.Equal(originalRowVersion, productB.RowVersion); // Ensure the RowVersion matches the original

            await using var verifyContext = _fixture.CreateContext();
            try
            {
                productA.Name = "Updated by context A";
                await contextA.SaveChangesAsync();

                Assert.NotEqual(originalRowVersion, productA.RowVersion); // Ensure the RowVersion has changed after the update
                Assert.Equal(originalRowVersion, productB.RowVersion); // Ensure contextB still has the original RowVersion
                Assert.NotEqual(productA.RowVersion, productB.RowVersion); // Ensure the RowVersions are different between contextA and contextB

                productB.Name = "Updated by context B";
                // Asserting that saving changes in contextB throws a DbUpdateConcurrencyException due to the stale RowVersion
                await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => contextB.SaveChangesAsync());

                var updatedProduct = await verifyContext.Products.FindAsync(productId);

                Assert.NotNull(updatedProduct);
                Assert.Equal("Updated by context A", updatedProduct.Name);
                Assert.Equal(productA.RowVersion, updatedProduct.RowVersion); // Ensure the RowVersion matches the one from contextA
            }
            finally // Clean up the test data to maintain database integrity and avoid side effects on other tests
            {
                await setupContext.DisposeAsync();
                await contextA.DisposeAsync();
                await contextB.DisposeAsync();
                await verifyContext.DisposeAsync();

                await using var cleanupContext = _fixture.CreateContext();

                var productToRemove = await cleanupContext.Products.FindAsync(productId);
                Assert.NotNull(productToRemove);
                var supplierToRemove = await cleanupContext.Suppliers.FindAsync(supplierId);
                Assert.NotNull(supplierToRemove);
                var categoryToRemove = await cleanupContext.Categories.FindAsync(categoryId);
                Assert.NotNull(categoryToRemove);

                cleanupContext.Products.Remove(productToRemove);
                cleanupContext.Suppliers.Remove(supplierToRemove);
                cleanupContext.Categories.Remove(categoryToRemove);
                await cleanupContext.SaveChangesAsync();

                await cleanupContext.DisposeAsync();
            }
        }


        //Helper Methods
        private static async Task<Product> CreateTestProductAsync(InvManDBContext context)
        {
            var category = await CreateCategoryAsync(context);
            var supplier = await CreateSupplierAsync(context);
            return CreateTestProduct(category.ID, supplier.ID);
        }

        private static Product CreateTestProduct(int categoryId, int supplierId)
        {
            return new Product
            {
                Name = $"Test product {Guid.NewGuid():N}",
                Sku = $"SKU{Guid.NewGuid():N}"[..8],
                Description = "This is a test product.",
                CategoryID = categoryId,
                SupplierID = supplierId,
                Price = 9.99m,
                QuantityInStock = 100,
                ReorderLevel = 10,
                IsActive = true
            };
        }

        private static async Task<Category> CreateCategoryAsync(InvManDBContext context)
        {
            var category = new Category
            {
                Name = $"Test category {Guid.NewGuid():N}",
                Description = "Category created by a product repository test.",
                IsActive = true,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();
            return category;
        }

        private static async Task<Supplier> CreateSupplierAsync(InvManDBContext context)
        {
            var supplier = new Supplier
            {
                Name = $"Test supplier {Guid.NewGuid():N}",
                ContactName = "Repository Test",
                PhoneContact = "0400000000",
                EmailContact = $"{Guid.NewGuid():N}@example.com",
                Address = "Repository test address",
                IsActive = true,
                Created = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            await context.Suppliers.AddAsync(supplier);
            await context.SaveChangesAsync();
            return supplier;
        }
    }
}
