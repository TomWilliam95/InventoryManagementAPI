using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Repositories.InvMovementRepositories
{
    public interface IInventoryMovementRepository
    {
        // === GET ===
        Task<InventoryMovement?> GetMovementByIdAsync(int id);
        Task<IEnumerable<InventoryMovement>> GetAllMovementsAsync();
        Task<IEnumerable<InventoryMovement>> GetMovementsByProductIdAsync(int productId);
        Task<IEnumerable<InventoryMovement>> GetMovementsByUserIdAsync(int userId);
        Task<IEnumerable<InventoryMovement>> GetMovementsByTypeAsync(MovementType movementType);
        Task<IEnumerable<InventoryMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate);

        // === POST ===
        Task<InventoryMovement> AddMovementAsync(InventoryMovement movement);
    }
}
