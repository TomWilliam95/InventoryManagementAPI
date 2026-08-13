using InventoryManagementAPI.Models.DTO_s.SupplierContactDTO_s;
using InventoryManagementAPI.Repositories.SupplierContactRepositories;
using Moq;

namespace InventoryManagementAPI.Tests;

public class SupplierContactServiceTests
{
    private readonly Mock<ISupplierContactRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task CreateAsync_WhenValid_AddsContactAndSaves()
    {
        _repository.Setup(x => x.SupplierExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await Service().CreateAsync(1, new CreateSupplierContactRequestDTO
        {
            Name = "Jane Buyer",
            Email = "jane@example.com",
            IsPrimary = true
        });

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        _repository.Verify(x => x.AddAsync(It.Is<SupplierContact>(c => c.SupplierID == 1 && c.Name == "Jane Buyer"), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailExists_Returns400()
    {
        _repository.Setup(x => x.SupplierExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository.Setup(x => x.EmailExistsForSupplierAsync(1, "used@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await Service().CreateAsync(1, new CreateSupplierContactRequestDTO { Name = "Jane", Email = "used@example.com" });

        Assert.Equal(400, result.StatusCode);
        _repository.Verify(x => x.AddAsync(It.IsAny<SupplierContact>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenContactBelongsToDifferentSupplier_Returns404()
    {
        _repository.Setup(x => x.GetByIdAsync(2, 10, It.IsAny<CancellationToken>())).ReturnsAsync((SupplierContact?)null);

        var result = await Service().UpdateAsync(2, 10, new UpdateSupplierContactRequestDTO { Name = "Jane", RowVersion = new byte[8] });

        Assert.Equal(404, result.StatusCode);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetPrimaryAsync_WhenAnotherPrimaryExists_Returns400()
    {
        var contact = Contact(10, false);
        _repository.Setup(x => x.GetByIdAsync(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(contact);
        _repository.Setup(x => x.GetPrimaryBySupplierIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Contact(11, true));

        var result = await Service().SetPrimaryAsync(1, 10, new UpdateSupplierContactPrimaryRequestDTO { IsPrimary = true, RowVersion = contact.RowVersion });

        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_WhenValid_RemovesAndSavesContact()
    {
        var contact = Contact(10, false);
        _repository.Setup(x => x.GetByIdAsync(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var result = await Service().DeleteAsync(1, 10, new DeleteSupplierContactRequestDTO { RowVersion = contact.RowVersion });

        Assert.True(result.Success);
        _repository.Verify(x => x.Remove(contact), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private SupplierContactService Service() => new(_repository.Object, _unitOfWork.Object);
    private static SupplierContact Contact(int id, bool primary) => new() { ID = id, SupplierID = 1, Name = "Contact", IsPrimary = primary, IsActive = true, RowVersion = new byte[8] };
}
