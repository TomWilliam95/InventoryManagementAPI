using InventoryManagementAPI.Models.CoreModels;
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

        // === GET === \\
        public async Task<IEnumerable<InventoryMovement>> GetAllMovementsAsync()
        {
            return await MovementWithDetails().ToListAsync();
        }

        public async Task<InventoryMovement?> GetMovementByIdAsync(int id)
        {
            return await MovementWithDetails()
                .FirstOrDefaultAsync(m => m.ID == id);
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await MovementWithDetails()
                .Where(m => m.Created >= startDate && m.Created <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByProductIdAsync(int productId)
        {
            return await MovementWithDetails()
                .Where(m => m.ProductId == productId)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByTypeAsync(MovementType movementType)
        {
            return await MovementWithDetails()
                .Where(m => m.Movement == movementType)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByUserIdAsync(int userId)
        {
            return await MovementWithDetails()
                .Where(m => m.UserID == userId)
                .ToListAsync();
        }

        // === POST === \\
        public async Task<InventoryMovement> AddMovementAsync(InventoryMovement movement)
        {
            await _context.InventoryMovements.AddAsync(movement);
            await _context.SaveChangesAsync();
            return movement;
        }

        // === QUERY HELPER METHOD === \\
        private IQueryable<InventoryMovement> MovementWithDetails()
        {
            return _context.InventoryMovements
                .Include(m => m.User)
                .Include(m => m.Product);
        }
    }
}
