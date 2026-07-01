using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.MovementDTO_s;

namespace InventoryManagementAPI.Repositories.InvMovementRepositories
{
    public interface IInventoryMovementService
    {
        Task<ApiResponse<InventoryMovementResponseDTO>> RecordStockInAsync(CreateInventoryMovementRequestDTO dto);

        Task<ApiResponse<InventoryMovementResponseDTO>> RecordStockOutAsync(CreateInventoryMovementRequestDTO dto);

        Task<ApiResponse<InventoryMovementResponseDTO>> RecordAdjustmentAsync(CreateInventoryMovementRequestDTO dto);

        Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetProductMovementHistoryAsync(int productId);

        Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetAllMovementsAsync();

        Task<ApiResponse<InventoryMovementResponseDTO>> GetMovementByIdAsync(int movementId);
    }
}
