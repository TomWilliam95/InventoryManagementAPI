using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Repositories.InventoryStockRepositories
{
    public interface IInventoryStockRepository
    {
        Task<IEnumerable<InventoryStock>> GetAllStockAsync(CancellationToken cancellationToken);

        Task<InventoryStock?> GetStockByProductAndWarehouseIDAsync(int productId, int warehouseId, CancellationToken cancellationToken);
        Task<InventoryStock?> GetStockByIdAsync(int stockId, CancellationToken cancellationToken);

        
        Task<IEnumerable<InventoryStock>> GetAllStockByWarehouseAsync(int warehouseId, CancellationToken cancellationToken);
        Task<IEnumerable<InventoryStock>> GetAllStockByProductAsync(int productId, CancellationToken cancellationToken);

        
        Task<IEnumerable<InventoryStock>> GetStockBelowReorderLevelAsync(CancellationToken cancellationToken);

        Task<InventoryStock> CreateInventoryStockAsync(InventoryStock stock, CancellationToken cancellationToken);
    }
}
