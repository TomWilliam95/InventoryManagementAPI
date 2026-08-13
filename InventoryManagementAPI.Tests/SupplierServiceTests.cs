using InventoryManagementAPI.Models.DTO_s.SupplierDTO_s;
using InventoryManagementAPI.Repositories.SupplierRepositories;
using Moq;

namespace InventoryManagementAPI.Tests;

public class SupplierServiceTests
{
    private readonly Mock<ISupplierRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task GetSupplierByIdAsync_WhenSupplierExists_ReturnsMappedSupplier()
    {
        _repository.Setup(x => x.GetSupplierByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Supplier(1));

        var result = await CreateService().GetSupplierByIdAsync(1);

        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Test Supplier", result.Data?.Name);
    }

    [Fact]
    public async Task CreateSupplierAsync_WhenNameAlreadyExists_Returns400AndDoesNotSave()
    {
        _repository.Setup(x => x.SupplierNameExistsAsync("Duplicate", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateService().CreateSupplierAsync(new CreateSupplierRequestDTO { Name = "Duplicate" });

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSupplierAsync_WhenValid_AddsAndSavesSupplier()
    {
        _repository.Setup(x => x.CreateSupplierAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier supplier, CancellationToken _) => supplier);

        var result = await CreateService().CreateSupplierAsync(new CreateSupplierRequestDTO { Name = " New Supplier ", IsActive = true });

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        _repository.Verify(x => x.CreateSupplierAsync(It.Is<Supplier>(s => s.Name == "New Supplier"), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSupplierAsync_WhenRowVersionDoesNotMatch_Returns409()
    {
        _repository.Setup(x => x.GetSupplierByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Supplier(1, [1, 2, 3, 4, 5, 6, 7, 8]));

        var result = await CreateService().UpdateSupplierAsync(1, new UpdateSupplierRequestDTO
        {
            Name = "Updated",
            RowVersion = [8, 7, 6, 5, 4, 3, 2, 1]
        });

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeactivateSupplierAsync_WhenValid_UpdatesStatusAndSaves()
    {
        var supplier = Supplier(1);
        _repository.Setup(x => x.GetSupplierByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(supplier);

        var result = await CreateService().DeactivateSupplierAsync(1, new UpdateSupplierStatusRequestDTO
        {
            IsActive = false,
            RowVersion = supplier.RowVersion
        });

        Assert.True(result.Success);
        Assert.False(supplier.IsActive);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private SupplierService CreateService() => new(_repository.Object, _unitOfWork.Object);
    private static Supplier Supplier(int id, byte[]? rowVersion = null) => new() { ID = id, Name = "Test Supplier", IsActive = true, RowVersion = rowVersion ?? new byte[8] };
}
