using InventoryManagementAPI.Models.CoreModels;
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
    public class ProductServiceTests
    {
        // === GET SINGLE PRODUCT TESTS ===
        [Fact]
        public async Task GetSingleProduct_GetProductSuccess_Return200()
        {
            // Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();

            productRepository.Setup(repo => repo.GetProductAsync(It.IsAny<int>()))
                .ReturnsAsync(CreateProduct(It.IsAny<string>()));

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
                .ReturnsAsync(new Product
                {
                    ID = It.IsAny<int>(),
                    Sku = "TestSku",
                    Name = "Test Product",
                    Description = "Test Description",
                    CategoryID = It.IsAny<int>(),
                    SupplierID = It.IsAny<int>(),
                    Category = null, // Simulate missing category data
                    Supplier = new Supplier
                    {
                        ID = It.IsAny<int>(),
                        Name = It.IsAny<string>(),
                        ContactName = It.IsAny<string>(),
                        PhoneContact = It.IsAny<string>(),
                        EmailContact = It.IsAny<string>(),
                        Address = It.IsAny<string>()
                    }
                });

            // Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetSingleProduct(123);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Product Successfully Retrieved, failed to retrieve either category or supplier details", result.Message);
        }

        [Fact]
        public async Task GetSingleProduct_GetProductSuccessWithoutSupplierData_Return500()
        {
            //Arrange
            var (productRepository, supplierRepository, categoryRepository) = CreateMockRepo();
            productRepository.Setup(repo => repo.GetProductAsync(It.IsAny<int>()))
                .ReturnsAsync(new Product
                {
                    ID = It.IsAny<int>(),
                    Sku = "TestSku",
                    Name = "Test Product",
                    Description = "Test Description",
                    CategoryID = It.IsAny<int>(),
                    SupplierID = It.IsAny<int>(),
                    Category = new Category
                    {
                        ID = It.IsAny<int>(),
                        Name = It.IsAny<string>(),
                        Description = It.IsAny<string>()
                    },
                    Supplier = null // Simulate missing supplier data
                });

            // Act
            var service = new ProductService(productRepository.Object, supplierRepository.Object, categoryRepository.Object);
            var result = await service.GetSingleProduct(123);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Product Successfully Retrieved, failed to retrieve either category or supplier details", result.Message);
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
                CreateProduct(It.IsAny<string>()),
                CreateProduct(It.IsAny<string>()),
                CreateProduct(It.IsAny<string>())
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


        // === CREATE MOQ REPO HELPER METHOD === \\
        private (Mock<IProductRepository>, Mock<ISupplierRepository>, Mock<ICategoryRepository>) CreateMockRepo()
        {
            var productRepoMock = new Mock<IProductRepository>();
            var supplierRepoMock = new Mock<ISupplierRepository>();
            var categoryRepoMock = new Mock<ICategoryRepository>();
            return (productRepoMock, supplierRepoMock, categoryRepoMock);
        }

        // === CREATE PRODUCT HELPER METHOD === \\
        private Product CreateProduct(string testName)
        {
            return new Product
            {
                ID = It.IsAny<int>(),
                Sku = testName,
                Name = testName,
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
    }
}
