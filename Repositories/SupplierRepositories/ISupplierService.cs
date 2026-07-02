using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierDTO_s;

namespace InventoryManagementAPI.Repositories.SupplierRepositories
{
    public interface ISupplierService
    {
        // === GET === \\
        Task<ApiResponse<IEnumerable<SupplierResponseDTO>>> GetAllSuppliersAsync();
        Task<ApiResponse<SupplierResponseDTO>> GetSupplierByIdAsync(int supplierId);

        // === POST === \\
        Task<ApiResponse<SupplierResponseDTO>> CreateSupplierAsync(CreateSupplierRequestDTO supplier);

        // === PUT === \\
        Task<ApiResponse<SupplierResponseDTO>> UpdateSupplierAsync(int supplierId, UpdateSupplierRequestDTO updatedSupplier);

        // === DELETE === \\
        Task<ApiResponse<object>> DeleteSupplierAsync(int supplierId);
    }
}
