using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierDTO_s;

namespace InventoryManagementAPI.Repositories.SupplierRepositories
{
    public interface ISupplierService
    {
        Task<ApiResponse<IEnumerable<SupplierResponseDTO>>> GetAllSuppliersAsync();
        Task<ApiResponse<SupplierResponseDTO>> GetSupplierByIdAsync(int supplierId);
        Task<ApiResponse<SupplierResponseDTO>> CreateSupplierAsync(CreateSupplierRequestDTO supplier);
        Task<ApiResponse<SupplierResponseDTO>> UpdateSupplierAsync(int supplierId, UpdateSupplierRequestDTO updatedSupplier);
        Task<ApiResponse<object>> DeleteSupplierAsync(int supplierId);
    }
}
