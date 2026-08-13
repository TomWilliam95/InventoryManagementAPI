using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierDTO_s;

namespace InventoryManagementAPI.Repositories.SupplierRepositories;

public interface ISupplierService
{
    Task<ApiResponse<IEnumerable<SupplierResponseDTO>>> GetAllSuppliersAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierResponseDTO>> GetSupplierByIdAsync(int supplierId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierResponseDTO>> CreateSupplierAsync(CreateSupplierRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierResponseDTO>> UpdateSupplierAsync(int supplierId, UpdateSupplierRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierResponseDTO>> ActivateSupplierAsync(int supplierId, UpdateSupplierStatusRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierResponseDTO>> DeactivateSupplierAsync(int supplierId, UpdateSupplierStatusRequestDTO dto, CancellationToken cancellationToken = default);
}
