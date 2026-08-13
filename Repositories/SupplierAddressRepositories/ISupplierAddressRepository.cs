using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Repositories.SupplierAddressRepositories
{
    public interface ISupplierAddressRepository
    {
        Task<SupplierAddress?> GetByIdAsync(int supplierId, int addressId, CancellationToken cancellationToken = default);
        Task<IEnumerable<SupplierAddress>> GetAllBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<IEnumerable<SupplierAddress>> GetByTypeAsync(int supplierId, SupplierAddressType type, CancellationToken cancellationToken = default);
        Task<SupplierAddress?> GetPrimaryByTypeAsync(int supplierId, SupplierAddressType type, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int supplierId, int addressId, CancellationToken cancellationToken = default);
        Task<bool> SupplierExistsAsync(int supplierId, CancellationToken cancellationToken = default);
        Task AddAsync(SupplierAddress address, CancellationToken cancellationToken = default);
    }
}
