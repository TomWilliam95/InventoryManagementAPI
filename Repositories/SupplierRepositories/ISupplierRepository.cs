using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Repositories.SupplierRepositories
{
    public interface ISupplierRepository
    {
        // === GET === \\
        Task<IEnumerable<Supplier>> GetAllSuppliersAsync();
        Task<Supplier?> GetSupplierByIdAsync(int supplierId);

        // === POST === \\
        Task<Supplier> CreateSupplierAsync(Supplier supplier);

        // === PUT === \\
        Task UpdateSupplierAsync(Supplier updatedSupplier);

        // === DELETE === \\
        Task DeleteSupplierAsync(int supplierId);

        // === Check Exists === \\
        Task<bool> SupplierExistsAsync(int supplierId);
    }
}
