using InventoryManagementAPI.Models.DTO_s.SupplierProductDTO_s;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Repositories.SupplierProductRepositories;
using Moq;

namespace InventoryManagementAPI.Tests;

public class SupplierProductServiceTests
{
    private readonly Mock<ISupplierProductRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task AssignAsync_WhenSupplierDoesNotExist_Returns404()
    {
        var result = await Service().AssignAsync(1, CreateAssignment());

        Assert.Equal(404, result.StatusCode);
        _repository.Verify(x => x.AddAsync(It.IsAny<SupplierProduct>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignAsync_WhenAssignmentAlreadyExists_Returns400()
    {
        SetupActiveSupplierAndProduct();
        _repository.Setup(x => x.AssignmentExistsAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await Service().AssignAsync(1, CreateAssignment());

        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task AssignAsync_WhenValid_AddsAssignmentAndSaves()
    {
        SetupActiveSupplierAndProduct();
        _repository.Setup(x => x.GetAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(Product());

        var result = await Service().AssignAsync(1, CreateAssignment());

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        _repository.Verify(x => x.AddAsync(It.Is<SupplierProduct>(p => p.SupplierID == 1 && p.ProductID == 2), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetPreferredAsync_WhenAnotherPreferredSupplierExists_Returns400()
    {
        var assignment = Product();
        assignment.IsPreferred = false;
        _repository.Setup(x => x.GetAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(assignment);
        _repository.Setup(x => x.GetPreferredSupplierForProductAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SupplierProduct { SupplierID = 9, ProductID = 2, MinimumOrderQuantity = 1 });

        var result = await Service().SetPreferredAsync(1, 2, new UpdateSupplierProductPreferredRequestDTO { IsPreferred = true, RowVersion = assignment.RowVersion });

        Assert.Equal(400, result.StatusCode);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeactivateAsync_WhenValid_UpdatesStatusAndSaves()
    {
        var assignment = Product();
        _repository.Setup(x => x.GetAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(assignment);

        var result = await Service().DeactivateAsync(1, 2, new UpdateSupplierProductStatusRequestDTO { IsActive = false, RowVersion = assignment.RowVersion });

        Assert.True(result.Success);
        Assert.False(assignment.IsActive);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private SupplierProductService Service() => new(_repository.Object, _unitOfWork.Object);
    private void SetupActiveSupplierAndProduct()
    {
        _repository.Setup(x => x.SupplierExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository.Setup(x => x.ProductExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }
    private static CreateSupplierProductRequestDTO CreateAssignment() => new() { ProductID = 2, SupplierSku = "SUP-2", UnitCost = 10, LeadTimeDays = 3, MinimumOrderQuantity = 1 };
    private static SupplierProduct Product() => new()
    {
        SupplierID = 1, ProductID = 2, SupplierSku = "SUP-2", UnitCost = 10, MinimumOrderQuantity = 1,
        IsActive = true, RowVersion = new byte[8], Supplier = new Supplier { ID = 1, Name = "Supplier" },
        Product = new Product { ID = 2, Sku = "P-2", Name = "Product", Description = "Test", CategoryID = 1 }
    };
}
