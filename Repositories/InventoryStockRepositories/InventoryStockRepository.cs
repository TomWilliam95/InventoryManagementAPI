using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.InventoryStockRepositories
{
    public class InventoryStockRepository: IInventoryStockRepository
    {
        private readonly InvManDBContext _context;
        public InventoryStockRepository(InvManDBContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<InventoryStock>> GetAllStockAsync(CancellationToken cancellationToken)
        {
            return await StockWithDetails().AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<InventoryStock>> GetAllStockByWarehouseAsync(int warehouseId, CancellationToken cancellationToken)
        {
            return await StockWithDetails().AsNoTracking().Where(stock => stock.WarehouseID == warehouseId).ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<InventoryStock>> GetAllStockByProductAsync(int productId, CancellationToken cancellationToken)
        {
            return await StockWithDetails().AsNoTracking().Where(stock => stock.ProductID == productId).ToListAsync(cancellationToken);
        }

        public async Task<InventoryStock?> GetStockByProductAndWarehouseIDAsync(int productId, int warehouseId, CancellationToken cancellationToken)
        {
            return await StockWithDetails().FirstOrDefaultAsync(stock => stock.ProductID == productId && stock.WarehouseID == warehouseId, cancellationToken);
        }
        
        public async Task<IEnumerable<InventoryStock>> GetStockBelowReorderLevelAsync(CancellationToken cancellationToken)
        {
            return await StockWithDetails().AsNoTracking().Where(stock => stock.Quantity <= stock.ReorderLevel).ToListAsync(cancellationToken);
        }


        public async Task<InventoryStock> CreateInventoryStockAsync(InventoryStock stock, CancellationToken cancellationToken)
        {
            await _context.InventoryStocks.AddAsync(stock, cancellationToken);
            return stock;
        }

        public async Task<InventoryStock?> GetStockByIdAsync(int stockId, CancellationToken cancellationToken)
        {
            return await StockWithDetails().FirstOrDefaultAsync(stock => stock.ID == stockId, cancellationToken);
        }

        private IQueryable<InventoryStock> StockWithDetails() => _context.InventoryStocks
            .Include(stock => stock.Product)
            .Include(stock => stock.Warehouse);
    }
}
