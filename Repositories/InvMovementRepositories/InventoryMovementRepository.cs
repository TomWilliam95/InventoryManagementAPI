using InventoryManagementAPI.Models.CoreModels.MovementModels;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.InvMovementRepositories
{
    public class InventoryMovementRepository : IInventoryMovementRepository
    {
        private readonly InvManDBContext _context;

        public InventoryMovementRepository(InvManDBContext context)
        {
            _context = context;
        }

        // === GET ===
        public async Task<IEnumerable<InventoryMovement>> GetAllMovementsAsync(CancellationToken cancellationToken = default)
        {
            return await MovementWithDetails().AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<InventoryMovement?> GetMovementByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await MovementWithDetails()
                .FirstOrDefaultAsync(m => m.ID == id, cancellationToken);
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await MovementWithDetails()
                .Where(m => m.Created >= startDate && m.Created <= endDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await MovementWithDetails()
                .Where(m => m.InventoryStock.ProductID == productId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByTypeAsync(MovementType movementType, CancellationToken cancellationToken = default)
        {
            return await MovementWithDetails()
                .Where(m => m.Movement == movementType)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await MovementWithDetails()

                .Where(m => m.UserID == userId)
                .ToListAsync(cancellationToken);
        }

        // === POST ===
        public async Task<InventoryMovement> AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken = default)
        {
            await _context.InventoryMovements.AddAsync(movement, cancellationToken);
            return movement;
        }

        // === QUERY HELPER METHOD ===
        private IQueryable<InventoryMovement> MovementWithDetails()
        {
            return _context.InventoryMovements
                .AsNoTracking()
                .Include(m => m.User)
                .Include(m => m.InventoryStock)
                    .ThenInclude(stock => stock.Product)
                .Include(m => m.InventoryStock)
                    .ThenInclude(stock => stock.Warehouse);
        }
    }
}
