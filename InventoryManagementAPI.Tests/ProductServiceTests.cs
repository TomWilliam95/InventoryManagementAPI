using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PATCH;
using InventoryManagementAPI.Repositories.CategoryRepositories;
using InventoryManagementAPI.Repositories.ProductRepositories;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace InventoryManagementAPI.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private ProductService CreateService() => new(
        _products.Object,
        _categories.Object,
        _unitOfWork.Object);

    [Fact]
    public async Task GetSingleProduct_ExistingProduct_Returns200()
    {
        _products.Setup(repository => repository.GetProductAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProduct());

        var result = await CreateService().GetSingleProduct(1);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Product Successfully Retrieved", result.Message);
        Assert.Equal("Test Product", result.Data!.Name);
    }

    [Fact]
    public async Task GetSingleProduct_MissingProduct_Returns404()
    {
        _products.Setup(repository => repository.GetProductAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await CreateService().GetSingleProduct(99);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Product Not Found", result.Message);
    }

    [Fact]
    public async Task GetSingleProduct_CategoryNotLoaded_Returns500()
    {
        var product = CreateProduct();
        product.Category = null;
        _products.Setup(repository => repository.GetProductAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await CreateService().GetSingleProduct(1);

        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task GetAllProducts_ProductsExist_Returns200()
    {
        _products.Setup(repository => repository.GetAllProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateProduct(), CreateProduct(2)]);

        var result = await CreateService().GetAllProducts();

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count());
    }

    [Fact]
    public async Task GetAllProducts_NoProducts_Returns404()
    {
        _products.Setup(repository => repository.GetAllProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await CreateService().GetAllProducts();

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("No Products Found", result.Message);
    }

    [Fact]
    public async Task GetProductsByCategory_MissingCategory_Returns404()
    {
        _categories.Setup(repository => repository.CategoryExistsAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await CreateService().GetProductsByCategory(9);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        _products.Verify(repository => repository.GetProductsByCategoryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetProductsBelowReorderLevel_ProductsExist_Returns200()
    {
        _products.Setup(repository => repository.GetProductsBelowReorderLevelAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateProduct()]);

        var result = await CreateService().GetProductsBelowReorderLevel();

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task AddProduct_ValidRequest_Returns201AndSaves()
    {
        var product = CreateProduct();
        _products.Setup(repository => repository.ProductSkuExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _products.Setup(repository => repository.ProductNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _categories.Setup(repository => repository.CategoryExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _products.Setup(repository => repository.AddProductAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _products.Setup(repository => repository.GetProductAsync(product.ID, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await CreateService().AddProduct(CreateRequest());

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddProduct_DuplicateSku_Returns400WithoutSaving()
    {
        _products.Setup(repository => repository.ProductSkuExistsAsync("SKU00001", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateService().AddProduct(CreateRequest());

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddProduct_MissingCategory_Returns404WithoutSaving()
    {
        _categories.Setup(repository => repository.CategoryExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await CreateService().AddProduct(CreateRequest());

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProductDetails_ValidRequest_Returns200AndSaves()
    {
        var product = CreateProduct();
        _products.Setup(repository => repository.GetProductAsync(product.ID, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _products.Setup(repository => repository.OtherProductNameExistsAsync(product.ID, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _products.Setup(repository => repository.OtherProductSkuExistsAsync(product.ID, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _categories.Setup(repository => repository.CategoryExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var result = await CreateService().UpdateProductDetails(product.ID, new UpdateProductDetailsRequestDTO
        {
            Sku = "SKU00002",
            Name = "Updated Product",
            Description = "Updated product description",
            CategoryID = 2,
            RowVersion = CreateRowVersion()
        });

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Updated Product", product.Name);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductDetails_RowVersionMismatch_Returns409()
    {
        _products.Setup(repository => repository.GetProductAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateProduct());

        var request = new UpdateProductDetailsRequestDTO
        {
            Sku = "SKU00002",
            Name = "Updated Product",
            Description = "Updated product description",
            CategoryID = 2,
            RowVersion = new byte[8]
        };

        var result = await CreateService().UpdateProductDetails(1, request);

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProductPrice_ValidRequest_UpdatesPriceAndSaves()
    {
        var product = CreateProduct();
        _products.Setup(repository => repository.GetProductAsync(product.ID, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await CreateService().UpdateProductPrice(product.ID, new UpdateProductPriceRequestDTO
        {
            Price = 25m,
            RowVersion = CreateRowVersion()
        });

        Assert.True(result.Success);
        Assert.Equal(25m, product.Price);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductPrice_ConcurrencyException_Returns409()
    {
        _products.Setup(repository => repository.GetProductAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateProduct());
        _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var result = await CreateService().UpdateProductPrice(1, new UpdateProductPriceRequestDTO
        {
            Price = 25m,
            RowVersion = CreateRowVersion()
        });

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task DeactivateProduct_ActiveProduct_DeactivatesAndSaves()
    {
        var product = CreateProduct();
        _products.Setup(repository => repository.GetProductAsync(product.ID, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await CreateService().DeactivateProduct(product.ID, new UpdateProductStatusRequestDTO
        {
            IsActive = true,
            RowVersion = CreateRowVersion()
        });

        Assert.True(result.Success);
        Assert.False(product.IsActive);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Product CreateProduct(int id = 1) => new()
    {
        ID = id,
        Sku = $"SKU{id:00000}",
        Name = "Test Product",
        Description = "A valid test product description",
        CategoryID = 1,
        Category = new Category { ID = 1, Name = "Test Category", Description = "Test category" },
        Price = 10m,
        IsActive = true,
        RowVersion = CreateRowVersion()
    };

    private static CreateProductRequestDTO CreateRequest() => new()
    {
        Sku = "SKU00001",
        Name = "Test Product",
        Description = "A valid test product description",
        CategoryID = 1,
        Price = 10m,
        IsActive = true
    };

    private static byte[] CreateRowVersion() => [1, 2, 3, 4, 5, 6, 7, 8];
}
