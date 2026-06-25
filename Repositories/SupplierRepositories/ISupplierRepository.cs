using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Repositories.SupplierRepositories
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<Supplier>> GetAllSuppliersAsync();
        Task<Supplier> GetSupplierByIdAsync(int supplierId);
        Task<Supplier> CreateSupplierAsync(Supplier supplier);
        Task<bool> UpdateSupplierAsync(int supplierId, Supplier updatedSupplier);
        Task<bool> DeleteSupplierAsync(int supplierId);
        Task<bool> SupplierExistsAsync(int supplierId);
    }
}
