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

        // === SET ACTIVE STATUS === \\
        Task<ApiResponse<SupplierResponseDTO>> ActivateSupplierAsync(int supplierId);
        Task<ApiResponse<SupplierResponseDTO>> DeactivateSupplierAsync(int supplierId);
    }
}
