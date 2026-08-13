using InventoryManagementAPI.Models.CoreModels.MovementModels;
using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Repositories.InvMovementRepositories
{
    public interface IInventoryMovementRepository
    {
        // === GET ===
        Task<InventoryMovement?> GetMovementByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryMovement>> GetAllMovementsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryMovement>> GetMovementsByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryMovement>> GetMovementsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryMovement>> GetMovementsByTypeAsync(MovementType movementType, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        // === POST ===
        Task<InventoryMovement> AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken = default);
    }
}
