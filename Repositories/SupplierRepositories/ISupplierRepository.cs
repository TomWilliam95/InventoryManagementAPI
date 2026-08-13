using InventoryManagementAPI.Models.CoreModels.SupplierModels;

namespace InventoryManagementAPI.Repositories.SupplierRepositories
{
    public interface ISupplierRepository
    {
        // === GET ===
        Task<IEnumerable<Supplier>> GetAllSuppliersAsync(CancellationToken cancellationToken = default);
        Task<Supplier?> GetSupplierByIdAsync(int supplierId, CancellationToken cancellationToken = default);

        // === POST ===
        Task<Supplier> CreateSupplierAsync(Supplier supplier, CancellationToken cancellationToken = default);

        // === Check Exists ===
        Task<bool> SupplierExistsAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<bool> SupplierNameExistsAsync(string supplierName, CancellationToken cancellationToken = default);
        Task<bool> SupplierNameExistsForOtherSupplierAsync(int supplierId, string supplierName, CancellationToken cancellationToken = default);
    }
}
