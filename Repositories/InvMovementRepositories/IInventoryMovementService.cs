using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.MovementDTO_s;
using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Repositories.InvMovementRepositories
{
    public interface IInventoryMovementService
    {
        // === GET === \\
        Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetProductMovementHistoryAsync(int productId);
        Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetAllMovementsAsync();
        Task<ApiResponse<InventoryMovementResponseDTO>> GetMovementByIdAsync(int movementId);
        Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetMovementsByUserIdAsync(int userId);
        Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetMovementsByMovementTypeAsync(MovementType movementType);

        // === POST === \\
        Task<ApiResponse<InventoryMovementResponseDTO>> RecordStockInAsync(CreateInventoryMovementRequestDTO dto);
        Task<ApiResponse<InventoryMovementResponseDTO>> RecordStockOutAsync(CreateInventoryMovementRequestDTO dto);
        Task<ApiResponse<InventoryMovementResponseDTO>> RecordAdjustmentAsync(CreateInventoryMovementRequestDTO dto);
    }
}
