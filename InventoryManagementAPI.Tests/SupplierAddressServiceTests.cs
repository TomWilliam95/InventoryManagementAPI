using InventoryManagementAPI.Models.DTO_s.SupplierAddressDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.SupplierAddressRepositories;
using Moq;

namespace InventoryManagementAPI.Tests;

public class SupplierAddressServiceTests
{
    private readonly Mock<ISupplierAddressRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task GetAllAsync_WhenSupplierIsInactiveOrMissing_Returns404()
    {
        var result = await Service().GetAllAsync(1);

        Assert.Equal(404, result.StatusCode);
        _repository.Verify(x => x.GetAllBySupplierIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenValid_AddsNormalisedAddressAndSaves()
    {
        _repository.Setup(x => x.SupplierExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await Service().CreateAsync(1, CreateAddress());

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        _repository.Verify(x => x.AddAsync(It.Is<SupplierAddress>(a => a.SupplierID == 1 && a.CountryCode == "AU"), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenPrimaryOfSameTypeExists_Returns400()
    {
        _repository.Setup(x => x.SupplierExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository.Setup(x => x.GetPrimaryByTypeAsync(1, SupplierAddressType.Billing, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Address(5));

        var result = await Service().CreateAsync(1, CreateAddress());

        Assert.Equal(400, result.StatusCode);
        _repository.Verify(x => x.AddAsync(It.IsAny<SupplierAddress>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenRowVersionDoesNotMatch_Returns409()
    {
        var address = Address(5);
        _repository.Setup(x => x.GetByIdAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(address);
        var dto = new UpdateSupplierAddressRequestDTO
        {
            Type = SupplierAddressType.Billing, AddressLine1 = "2 Test Street", City = "Brisbane",
            PostalCode = "4000", CountryCode = "AU", RowVersion = [1, 1, 1, 1, 1, 1, 1, 1]
        };

        var result = await Service().UpdateAsync(1, 5, dto);

        Assert.Equal(409, result.StatusCode);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private SupplierAddressService Service() => new(_repository.Object, _unitOfWork.Object);
    private static CreateSupplierAddressRequestDTO CreateAddress() => new() { Type = SupplierAddressType.Billing, AddressLine1 = "1 Test Street", City = "Brisbane", PostalCode = "4000", CountryCode = "au", IsPrimary = true };
    private static SupplierAddress Address(int id) => new() { ID = id, SupplierID = 1, Type = SupplierAddressType.Billing, AddressLine1 = "1 Test Street", City = "Brisbane", PostalCode = "4000", CountryCode = "AU", IsPrimary = true, IsActive = true, RowVersion = new byte[8] };
}
