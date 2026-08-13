using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.MovementDTO_s;
using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Repositories.InvMovementRepositories
{
    public interface IInventoryMovementService
    {
        // === GET ===
        Task<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>> GetProductMovementHistoryAsync(int productId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>> GetAllMovementsAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<InventoryMovementResponseDTO>> GetMovementByIdAsync(int movementId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>> GetMovementsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>> GetMovementsByMovementTypeAsync(MovementType movementType, CancellationToken cancellationToken = default);
        // === POST ===
        Task<ApiResponse<InventoryMovementResponseDTO>> RecordStockInAsync(CreateInventoryMovementRequestDTO dto, int userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<InventoryMovementResponseDTO>> RecordStockOutAsync(CreateInventoryMovementRequestDTO dto, int userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<InventoryMovementResponseDTO>> RecordAdjustmentAsync(CreateInventoryMovementRequestDTO dto, int userId, CancellationToken cancellationToken = default);
    }
}
