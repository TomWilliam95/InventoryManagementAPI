using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.WarehouseRepositories
{
    public class WarehouseRepository: IWarehouseRepository
    {
        private readonly InvManDBContext _context;
        public WarehouseRepository(InvManDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Warehouse>> GetAllWarehousesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Warehouses.ToListAsync(cancellationToken);
        }

        public async Task<Warehouse?> GetWarehouseByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Warehouses.SingleOrDefaultAsync(w => w.ID == id, cancellationToken);
        }

        public async Task<Warehouse> AddWarehouseAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
        {
            await _context.Warehouses.AddAsync(warehouse, cancellationToken);
            return warehouse;
        }

        public async Task<bool> IsWarehouseActiveAsync(int warehouseId, CancellationToken cancellationToken = default)
        {
            var warehouse = await _context.Warehouses.SingleOrDefaultAsync(w => w.ID == warehouseId, cancellationToken);
            return warehouse?.IsActive ?? false;
        }
    }
}
