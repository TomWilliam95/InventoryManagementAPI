using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierContactDTO_s;

namespace InventoryManagementAPI.Repositories.SupplierContactRepositories;

public interface ISupplierContactService
{
    Task<ApiResponse<IEnumerable<SupplierContactResponseDTO>>> GetAllAsync(int supplierId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierContactResponseDTO>> GetByIdAsync(int supplierId, int contactId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierContactResponseDTO>> GetPrimaryAsync(int supplierId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierContactResponseDTO>> CreateAsync(int supplierId, CreateSupplierContactRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierContactResponseDTO>> UpdateAsync(int supplierId, int contactId, UpdateSupplierContactRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierContactResponseDTO>> SetPrimaryAsync(int supplierId, int contactId, UpdateSupplierContactPrimaryRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierContactResponseDTO>> ActivateAsync(int supplierId, int contactId, UpdateSupplierContactStatusRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierContactResponseDTO>> DeactivateAsync(int supplierId, int contactId, UpdateSupplierContactStatusRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(int supplierId, int contactId, DeleteSupplierContactRequestDTO dto, CancellationToken cancellationToken = default);
}
