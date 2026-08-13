using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierProductDTO_s;
namespace InventoryManagementAPI.Repositories.SupplierProductRepositories;
public interface ISupplierProductService
{
    Task<ApiResponse<IEnumerable<SupplierProductResponseDTO>>> GetAllBySupplierAsync(int supplierId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<SupplierProductResponseDTO>>> GetAllByProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierProductResponseDTO>> GetAsync(int supplierId, int productId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierProductResponseDTO>> AssignAsync(int supplierId, CreateSupplierProductRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierProductResponseDTO>> UpdateAsync(int supplierId, int productId, UpdateSupplierProductRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierProductResponseDTO>> SetPreferredAsync(int supplierId, int productId, UpdateSupplierProductPreferredRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierProductResponseDTO>> ActivateAsync(int supplierId, int productId, UpdateSupplierProductStatusRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierProductResponseDTO>> DeactivateAsync(int supplierId, int productId, UpdateSupplierProductStatusRequestDTO dto, CancellationToken cancellationToken = default);
}
