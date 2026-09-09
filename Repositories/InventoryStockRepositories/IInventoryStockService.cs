using InventoryManagementAPI.Models.Contracts.InventoryStocks;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.InventoryStockDTO_s;
using InventoryManagementAPI.Models.Shared;

namespace InventoryManagementAPI.Repositories.InventoryStockRepositories
{
    public interface IInventoryStockService
    {
        Task<ApiResponse<InventoryStockResponseDTO>> GetInventoryStockByProductAndWarehouseIdAsync(int productId, int warehouseId, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<BulkInventoryStockResponseDTO>>> GetInventoryStocksAsync(InventoryStockQueryParameters query, CancellationToken cancellationToken = default);

        Task<ApiResponse<InventoryStockResponseDTO>> CreateInventoryStockAsync(CreateInventoryStockRequestDTO dto, CancellationToken cancellationToken = default);

        Task<ApiResponse<InventoryStockResponseDTO>> UpdateReorderLevelAsync(int inventoryStockId, UpdateReorderLevelRequestDTO dto, CancellationToken cancellationToken = default);

        Task<ApiResponse<InventoryStockResponseDTO>> ActivateInventoryStockAsync(int inventoryStockId, UpdateInventoryStockStatusRequestDTO dto, CancellationToken cancellationToken = default);

        Task<ApiResponse<InventoryStockResponseDTO>> DeactivateInventoryStockAsync(int inventoryStockId, UpdateInventoryStockStatusRequestDTO dto, CancellationToken cancellationToken = default);
    }
}
