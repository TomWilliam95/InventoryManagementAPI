using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierAddressDTO_s;
using InventoryManagementAPI.Models.Enums;
namespace InventoryManagementAPI.Repositories.SupplierAddressRepositories;
public interface ISupplierAddressService
{
    Task<ApiResponse<IEnumerable<SupplierAddressResponseDTO>>> GetAllAsync(int supplierId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<SupplierAddressResponseDTO>>> GetByTypeAsync(int supplierId, SupplierAddressType type, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierAddressResponseDTO>> GetByIdAsync(int supplierId, int addressId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierAddressResponseDTO>> CreateAsync(int supplierId, CreateSupplierAddressRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierAddressResponseDTO>> UpdateAsync(int supplierId, int addressId, UpdateSupplierAddressRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierAddressResponseDTO>> SetPrimaryAsync(int supplierId, int addressId, UpdateSupplierAddressPrimaryRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierAddressResponseDTO>> ActivateAsync(int supplierId, int addressId, UpdateSupplierAddressStatusRequestDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierAddressResponseDTO>> DeactivateAsync(int supplierId, int addressId, UpdateSupplierAddressStatusRequestDTO dto, CancellationToken cancellationToken = default);
}
