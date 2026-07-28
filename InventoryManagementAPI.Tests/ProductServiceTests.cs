using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PUT.STOCK;
using InventoryManagementAPI.Repositories.CategoryRepositories;
using InventoryManagementAPI.Repositories.ProductRepositories;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using InventoryManagementAPI.Repositories.SupplierRepositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Tests
{
    // ProductService unit tests use mocked product, supplier, and category
    // repositories. This keeps each test focused on service validation and
    // response mapping rather than database behaviour.
    public class ProductServiceTests
    {
        // === GET SINGLE PRODUCT TESTS ===
        [Fact]
        public async Task GetSingleProduct_GetProductSuccess_Return200()
        {
            // Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();

            productRepository.Setup(repo => repo.GetProductAsync(It.IsAny<int>()))
                .ReturnsAsync(CreateProduct());

            // Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetSingleProduct(123);

            //Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Product Successfully Retrieved", result.Message);

            //Verify
            productRepository.Verify(repo => repo.GetProductAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetSingleProduct_GetProductNotFound_Return404()
        {
            // Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();

            productRepository.Setup(repo => repo.GetProductAsync(It.IsAny<int>()))
                .ReturnsAsync((Product?)null);

            // Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetSingleProduct(123);

            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Product Not Found", result.Message);

            //Verify
            productRepository.Verify(repo => repo.GetProductAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetSingleProduct_GetProductSuccessWithoutCategoryData_Return500()
        {
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();

            productRepository.Setup(repo => repo.GetProductAsync(It.IsAny<int>()))
                .ReturnsAsync(CreateProductNullCategory());

            // Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetSingleProduct(123);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Product Successfully Retrieved, failed to retrieve either category or supplier details", result.Message);

            // Verify
            productRepository.Verify(repo => repo.GetProductAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetSingleProduct_GetProductSuccessWithoutSupplierData_Return500()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            productRepository.Setup(repo => repo.GetProductAsync(It.IsAny<int>()))
                .ReturnsAsync(CreateProductNullSupplier());

            // Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetSingleProduct(123);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Product Successfully Retrieved, failed to retrieve either category or supplier details", result.Message);

            // Verify
            productRepository.Verify(repo => repo.GetProductAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetSingleProduct_GetProductThrowsException_Return500()
        {
            // Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();

            productRepository.Setup(repo => repo.GetProductAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database connection error"));

            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetSingleProduct(123);

            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to load product.", result.Message);

            // Verify
            productRepository.Verify(repo => repo.GetProductAsync(It.IsAny<int>()), Times.Once);
        }


        // === GET ALL PRODUCTS TESTS === \\
        [Fact]
        public async Task GetProducts_GetProductsSuccess_Return200()
        {
            // Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            productRepository.Setup(repo =>
            repo.GetAllProductsAsync()).
            ReturnsAsync(new List<Product>
            {
                CreateProduct(),
                CreateProduct(),
                CreateProduct()
            });

            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetAllProducts();

            //Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Successfully Retrieved All Products", result.Message);

            //Verify
            productRepository.Verify(repo => repo.GetAllProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProducts_GetProductsWhenNoProductsExist_Returns404()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            productRepository.Setup(repo => repo.GetAllProductsAsync())
                .ReturnsAsync([]);

            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetAllProducts();

            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No Products Found", result.Message);

            //Verify
            productRepository.Verify(repo => repo.GetAllProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProducts_GetProductsThrowsException_Return500()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            productRepository.Setup(repo => repo.GetAllProductsAsync())
                .ThrowsAsync(new Exception("Database connection error"));

            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetAllProducts();

            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to load all products.", result.Message);

            //Verify
            productRepository.Verify(repo => repo.GetAllProductsAsync(), Times.Once);
        }


        // === GET PRODUCTS BY CATEGORY TESTS === \\
        [Fact]
        public async Task GetProductsByCategory_Success_Return200()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            categoryRepository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            productRepository.Setup(repo => repo.GetProductsByCategoryAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Product>
                {
                    CreateProduct(),
                    CreateProduct(),
                    CreateProduct()
                });

            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetProductsByCategory(It.IsAny<int>());

            //Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Successfully Retrieved Products By Category", result.Message);

            //Verify
            categoryRepository.Verify(repo => repo.CategoryExistsAsync(It.IsAny<int>()), Times.Once);
            productRepository.Verify(repo => repo.GetProductsByCategoryAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetProductsByCategory_NullCategory_Return404()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            categoryRepository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetProductsByCategory(It.IsAny<int>());

            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Category Not Found", result.Message);

            //Verify
            categoryRepository.Verify(repo => repo.CategoryExistsAsync(It.IsAny<int>()), Times.Once);
            productRepository.Verify(repo => repo.GetProductsByCategoryAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetProductsByCategory_NullProducts_Return404()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            categoryRepository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            productRepository.Setup(repo => repo.GetProductsByCategoryAsync(It.IsAny<int>()))
                .ReturnsAsync([]);
            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetProductsByCategory(It.IsAny<int>());
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No Products Found", result.Message);
            //Verify
            categoryRepository.Verify(repo => repo.CategoryExistsAsync(It.IsAny<int>()), Times.Once);
            productRepository.Verify(repo => repo.GetProductsByCategoryAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetProductsByCategory_ThrowException_Return500()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            categoryRepository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            productRepository.Setup(repo => repo.GetProductsByCategoryAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database connection error"));
            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetProductsByCategory(It.IsAny<int>());
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to load products by category.", result.Message);
            //Verify
            categoryRepository.Verify(repo => repo.CategoryExistsAsync(It.IsAny<int>()), Times.Once);
            productRepository.Verify(repo => repo.GetProductsByCategoryAsync(It.IsAny<int>()), Times.Once);
        }

        // === GET PRODUCTS BELOW REORDER LEVEL TESTS === \\
        [Fact]
        public async Task GetProductsBelowReorderLevel_Success_Return200()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            productRepository.Setup(productRepository => productRepository.GetProductsBelowReorderLevelAsync())
                .ReturnsAsync(new List<Product>
                {
                    CreateProduct(),
                    CreateProduct(),
                    CreateProduct()
                });
            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetProductsBelowReorderLevel();
            //Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Successfully Retrieved Products Below Reorder Level", result.Message);
            //Verify
            productRepository.Verify(productRepository => productRepository.GetProductsBelowReorderLevelAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProductsBelowReorderLevel_NoProducts_Return404()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            productRepository.Setup(productRepository => productRepository.GetProductsBelowReorderLevelAsync())
                .ReturnsAsync([]);
            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetProductsBelowReorderLevel();
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No Products Found", result.Message);
            //Verify
            productRepository.Verify(productRepository => productRepository.GetProductsBelowReorderLevelAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProductsBelowReorderLevel_ThrowException_Return500()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            productRepository.Setup(productRepository => productRepository.GetProductsBelowReorderLevelAsync())
                .ThrowsAsync(new Exception("Database connection error"));
            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetProductsBelowReorderLevel();
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to load products below reorder level.", result.Message);
            //Verify
            productRepository.Verify(productRepository => productRepository.GetProductsBelowReorderLevelAsync(), Times.Once);
        }


        // ===  CREATE PRODUCT SERVICE HELPER METHOD === \\
        [Fact]
        public async Task CreateProduct_Success_Return201()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            categoryRepository.Setup(categoryRepository => categoryRepository.CategoryExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            supplierRepository.Setup(supplierRepository => supplierRepository.SupplierExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            productRepository.Setup(productRepository => productRepository.AddProductAsync(It.IsAny<Product>()))
                .ReturnsAsync(CreateProduct());
            productRepository.Setup(repo => repo.GetProductAsync(It.IsAny<int>()))
                .ReturnsAsync(CreateProduct());
            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.AddProduct(new CreateProductRequestDTO
            {
                Sku = "SKU12345",
                Name = "Test Product",
                Description = "Test Description",
                CategoryID = 1,
                SupplierID = 1,
                Price = 10.0m,
                QuantityInStock = 100,
                ReorderLevel = 5,
                IsActive = true
            });

            //Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("Product Successfully Created", result.Message);
            //Verify
            categoryRepository.Verify(categoryRepository => categoryRepository.CategoryExistsAsync(It.IsAny<int>()), Times.Once);
            supplierRepository.Verify(supplierRepository => supplierRepository.SupplierExistsAsync(It.IsAny<int>()), Times.Once);
            productRepository.Verify(productRepository => productRepository.AddProductAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task CreateProduct_NameDuplicate_Return400()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            productRepository.Setup(repo => repo.ProductNameExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.AddProduct(new CreateProductRequestDTO
            {
                Sku = "SKU12345",
                Name = "Test Product",
                Description = "Test Description",
                CategoryID = 1,
                SupplierID = 1,
                Price = 10.0m,
                ReorderLevel = 5
            });
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Product Name Already Exists", result.Message);
            //Verify
            productRepository.Verify(repo => repo.ProductNameExistsAsync(It.IsAny<string>()), Times.Once);
            productRepository.Verify(repo => repo.AddProductAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task CreateProduct_SkuDuplicate_Return400()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            productRepository.Setup(repo => repo.ProductSkuExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.AddProduct(new CreateProductRequestDTO
            {
                Sku = "SKU12345",
                Name = "Test Product",
                Description = "Test Description",
                CategoryID = 1,
                SupplierID = 1,
                Price = 10.0m,
                ReorderLevel = 5
            });
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Product with the same SKU already exists.", result.Message);
            //Verify
            productRepository.Verify(repo => repo.ProductSkuExistsAsync(It.IsAny<string>()), Times.Once);
            productRepository.Verify(repo => repo.AddProductAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task CreateProduct_CategoryNull_Return404()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            categoryRepository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(false);
            supplierRepository.Setup(repo => repo.SupplierExistsAsync(It.IsAny<int>())).
                ReturnsAsync(true);
            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.AddProduct(new CreateProductRequestDTO
            {
                Sku = "SKU12345",
                Name = "Test Product",
                Description = "Test Description",
                CategoryID = 1,
                SupplierID = 1,
                Price = 10.0m,
                ReorderLevel = 5
            });
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Category does not exist", result.Message);
            //Verify
            categoryRepository.Verify(repo => repo.CategoryExistsAsync(It.IsAny<int>()), Times.Once);
            productRepository.Verify(repo => repo.AddProductAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task CreateProduct_SupplierNull_Return404()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            categoryRepository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            supplierRepository.Setup(repo => repo.SupplierExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(false);
            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.AddProduct(new CreateProductRequestDTO
            {
                Sku = "SKU12345",
                Name = "Test Product",
                Description = "Test Description",
                CategoryID = 1,
                SupplierID = 1,
                Price = 10.0m,
                ReorderLevel = 5
            });
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Supplier does not exist", result.Message);
            //Verify
            supplierRepository.Verify(repo => repo.SupplierExistsAsync(It.IsAny<int>()), Times.Once);
            productRepository.Verify(repo => repo.AddProductAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task CreateProduct_InvalidPrice_Return400()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            supplierRepository.Setup(repo => repo.SupplierExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            categoryRepository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.AddProduct(new CreateProductRequestDTO
            {
                Sku = "SKU12345",
                Name = "Test Product",
                Description = "Test Description",
                CategoryID = 1,
                SupplierID = 1,
                Price = -10.0m,
                ReorderLevel = 5
            });

            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Price must be greater than zero", result.Message);
            //Verify
            productRepository.VerifyNoOtherCalls();
            supplierRepository.VerifyNoOtherCalls();
            categoryRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateProduct_InvalidData_Return400()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            supplierRepository.Setup(repo => repo.SupplierExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            categoryRepository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.AddProduct((CreateProductRequestDTO)null!);
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid DTO", result.Message);
            //Verify
            productRepository.VerifyNoOtherCalls();
            supplierRepository.VerifyNoOtherCalls();
            categoryRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateProduct_ThrowException_Return500()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo(); 
            supplierRepository.Setup(repo => repo.SupplierExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            categoryRepository.Setup(repo => repo.CategoryExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            productRepository.Setup(repo => repo.AddProductAsync(It.IsAny<Product>()))
                .ThrowsAsync(new Exception("Database connection error"));
            //Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.AddProduct(new CreateProductRequestDTO
            {
                Sku = "SKU12345",
                Name = "Test Product",
                Description = "Test Description",
                CategoryID = 1,
                SupplierID = 1,
                Price = 10.0m,
                ReorderLevel = 5
            });
            //Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Internal error occurred, failed to create product.", result.Message);
            //Verify
            productRepository.Verify(repo => repo.AddProductAsync(It.IsAny<Product>()), Times.Once);
        }

        // === UPDATE PRODUCT TESTS === \\
        [Fact]
    public async Task UpdateProductDetails_ValidRequest_Returns200()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            var product = CreateProduct();
            product.ID = 1;
            productRepo.Setup(repo => repo.GetProductAsync(1)).ReturnsAsync(product);
            supplierRepo.Setup(repo => repo.SupplierExistsAsync(2)).ReturnsAsync(true);
            categoryRepo.Setup(repo => repo.CategoryExistsAsync(2)).ReturnsAsync(true);
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.UpdateProductDetails(1, CreateUpdateProductDTO());

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Successfully Updated Product Details", result.Message);
            Assert.Equal("Updated Product", result.Data!.Name);

            // SaveChanges is the persistence boundary for this update. Times.Once
            // ensures the valid request was saved exactly once.
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
    public async Task UpdateProductDetails_NullRequest_Returns400()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.UpdateProductDetails(1, null!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid DTO", result.Message);

            // Validation failed before persistence, so saving would be incorrect.
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
    public async Task UpdateProductDetails_ProductNotFound_Returns404()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            productRepo.Setup(repo => repo.GetProductAsync(99)).ReturnsAsync((Product?)null);
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.UpdateProductDetails(99, CreateUpdateProductDTO());

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Product Not Found", result.Message);

            // Verify
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
    public async Task UpdateProductDetails_DuplicateName_Returns400()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            var product = CreateProduct();
            product.ID = 1;
            productRepo.Setup(repo => repo.GetProductAsync(1)).ReturnsAsync(product);
            productRepo.Setup(repo => repo.OtherProductNameExistsAsync(1, "Updated Product")).ReturnsAsync(true);
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.UpdateProductDetails(1, CreateUpdateProductDTO());

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Product Name Already Exists", result.Message);

            // Verify
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
    public async Task UpdateProductDetails_DuplicateSku_Returns400()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            var product = CreateProduct();
            product.ID = 1;
            productRepo.Setup(repo => repo.GetProductAsync(1)).ReturnsAsync(product);
            productRepo.Setup(repo => repo.OtherProductSkuExistsAsync(1, "NEWSKU01")).ReturnsAsync(true);
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.UpdateProductDetails(1, CreateUpdateProductDTO());

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Product SKU Already Exists", result.Message);

            // Verify
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
    public async Task UpdateProductPrice_ValidPrice_Returns200()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            var product = CreateProduct();
            productRepo.Setup(repo => repo.GetProductAsync(1)).ReturnsAsync(product);
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.UpdateProductPrice(1, new UpdateProductPriceRequestDTO { Price = 25.50m });

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Price Successfully Updated", result.Message);
            Assert.Equal(25.50m, result.Data!.Price);

            // Verify
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
    public async Task UpdateProductPrice_NonPositivePrice_Returns400()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.UpdateProductPrice(1, new UpdateProductPriceRequestDTO { Price = 0 });

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid Price Entry", result.Message);

            // Verify
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
    public async Task UpdateProductPrice_ProductNotFound_Returns404()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            productRepo.Setup(repo => repo.GetProductAsync(99)).ReturnsAsync((Product?)null);
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.UpdateProductPrice(99, new UpdateProductPriceRequestDTO { Price = 10 });

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Product Not Found", result.Message);

            // Verify
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
    public async Task UpdateProductReorderLevel_ValidLevel_Returns200()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            productRepo.Setup(repo => repo.GetProductAsync(1)).ReturnsAsync(CreateProduct());
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.UpdateProductReorderLevel(1,
                new UpdateProductReorderRequestDTO { ReorderLevel = 8 });

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("ReOrderStock Levels Updated", result.Message);
            Assert.Equal(8, result.Data!.ReorderLevel);

            // Verify
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
    public async Task UpdateProductReorderLevel_NegativeLevel_Returns400()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.UpdateProductReorderLevel(1,
                new UpdateProductReorderRequestDTO { ReorderLevel = -1 });

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid Reorder Level", result.Message);

            // Verify
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
    public async Task ActivateProduct_InactiveProduct_Returns200()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            var product = CreateProduct();
            product.IsActive = false;
            productRepo.Setup(repo => repo.GetProductAsync(1)).ReturnsAsync(product);
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.ActivateProduct(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Product activated successfully", result.Message);
            Assert.True(result.Data!.IsActive);

            // Verify
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
    public async Task ActivateProduct_AlreadyActive_Returns400()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            var product = CreateProduct();
            product.IsActive = true;
            productRepo.Setup(repo => repo.GetProductAsync(1)).ReturnsAsync(product);
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.ActivateProduct(1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Product is already active", result.Message);

            // Verify
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
    public async Task DeactivateProduct_ActiveProduct_Returns200()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            var product = CreateProduct();
            product.IsActive = true;
            productRepo.Setup(repo => repo.GetProductAsync(1)).ReturnsAsync(product);
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.DeactivateProduct(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Product deactivated successfully", result.Message);
            Assert.False(result.Data!.IsActive);

            // Verify
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
    public async Task DeactivateProduct_AlreadyInactive_Returns400()
    {
            // Arrange
            var (productRepo, supplierRepo, categoryRepo) = CreateMockRepo();
            var product = CreateProduct();
            product.IsActive = false;
            productRepo.Setup(repo => repo.GetProductAsync(1)).ReturnsAsync(product);
            var service = new ProductService(productRepo.Object, supplierRepo.Object, categoryRepo.Object);

            // Act
            var result = await service.DeactivateProduct(1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Product is already inactive", result.Message);

            // Verify
            productRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        // === CREATE MOQ REPO HELPER METHOD === \\
        private (Mock<IProductRepository>, Mock<ISupplierRepository>, Mock<ICategoryRepository>) CreateMockRepo()
        {
            // ProductService depends on three repositories. Returning their Mock
            // wrappers together avoids repeating this setup in every product test.
            var productRepoMock = new Mock<IProductRepository>();
            var supplierRepoMock = new Mock<ISupplierRepository>();
            var categoryRepoMock = new Mock<ICategoryRepository>();
            return (productRepoMock, supplierRepoMock, categoryRepoMock);
        }

        // === CREATE PRODUCT HELPER METHOD === \\
        private Product CreateProduct()
        {
            return new Product
            {
                ID = It.IsAny<int>(),
                Sku = It.IsAny<string>(),
                Name = It.IsAny<string>(),
                Description = It.IsAny<string>(),
                CategoryID = It.IsAny<int>(),
                SupplierID = It.IsAny<int>(),
                Category = new Category
                {
                    ID = It.IsAny<int>(),
                    Name = It.IsAny<string>(),
                    Description = It.IsAny<string>()
                },
                Supplier = new Supplier
                {
                    ID = It.IsAny<int>(),
                    Name = It.IsAny<string>(),
                    ContactName = It.IsAny<string>(),
                    PhoneContact = It.IsAny<string>(),
                    EmailContact = It.IsAny<string>(),
                    Address = It.IsAny<string>()
                }
            };
        }

        private Product CreateProductNullCategory()
        {
            return new Product
            {
                ID = It.IsAny<int>(),
                Sku = It.IsAny<string>(),
                Name = It.IsAny<string>(),
                Description = It.IsAny<string>(),
                CategoryID = It.IsAny<int>(),
                SupplierID = It.IsAny<int>(),
                Category = null,
                Supplier = new Supplier
                {
                    ID = It.IsAny<int>(),
                    Name = It.IsAny<string>(),
                    ContactName = It.IsAny<string>(),
                    PhoneContact = It.IsAny<string>(),
                    EmailContact = It.IsAny<string>(),
                    Address = It.IsAny<string>()
                }
            };
        }


        private Product CreateProductNullSupplier()
        {
            return new Product
            {
                ID = It.IsAny<int>(),
                Sku = It.IsAny<string>(),
                Name = It.IsAny<string>(),
                Description = It.IsAny<string>(),
                CategoryID = It.IsAny<int>(),
                SupplierID = It.IsAny<int>(),
                Category = new Category
                {
                    ID = It.IsAny<int>(),
                    Name = It.IsAny<string>(),
                    Description = It.IsAny<string>()
                },
                Supplier = null
            };
        }

        private UpdateProductDetailsRequestDTO CreateUpdateProductDTO()
        {
            // Valid update used as a baseline. Individual tests configure the mocks
            // to make a name, SKU, category, or supplier validation fail.
            return new UpdateProductDetailsRequestDTO
            {
                Sku = "NEWSKU01",
                Name = "Updated Product",
                Description = "Updated product description",
                CategoryID = 2,
                SupplierID = 2
            };
        }
    }
}
