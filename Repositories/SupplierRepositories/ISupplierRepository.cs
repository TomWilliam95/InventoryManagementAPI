using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Repositories.SupplierRepositories
{
    public interface ISupplierRepository
    {
        // === GET === \\
        Task<IEnumerable<Supplier>> GetAllSuppliersAsync();
        Task<Supplier> GetSupplierByIdAsync(int supplierId);

        // === POST === \\
        Task<Supplier> CreateSupplierAsync(Supplier supplier);

        // === PUT === \\
        Task<bool> UpdateSupplierAsync(int supplierId, Supplier updatedSupplier);

        // === DELETE === \\
        Task<bool> DeleteSupplierAsync(int supplierId);

        // === CHECK EXISTENCE === \\
        Task<bool> SupplierExistsAsync(int supplierId);
    }
}
