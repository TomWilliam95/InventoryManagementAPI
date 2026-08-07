using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Repositories.SupplierRepositories
{
    public interface ISupplierRepository
    {
        // === GET ===
        Task<IEnumerable<Supplier>> GetAllSuppliersAsync();
        Task<Supplier?> GetSupplierByIdAsync(int supplierId);

        // === POST ===
        Task<Supplier> CreateSupplierAsync(Supplier supplier);

        // === Check Exists ===
        Task<bool> SupplierExistsAsync(int supplierId);
        Task<bool> SupplierNameExistsAsync(string supplierName);
        Task<bool> SupplierNameExistsForOtherSupplierAsync(int supplierId, string supplierName);
        Task<bool> SupplierEmailExistsAsync(string supplierEmail);
        Task<bool> SupplierEmailExistsForOtherSupplierAsync(int supplierId, string supplierEmail);

        // === SAVE CHANGES ===
        Task SaveChangesAsync();
    }
}
