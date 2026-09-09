using InventoryManagementAPI.Models.Contracts.InventoryStocks;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.Shared;

namespace InventoryManagementAPI.Repositories.InventoryStockRepositories
{
    public interface IInventoryStockRepository
    {
        Task<PagedData<InventoryStock>> GetStockAsync(InventoryStockQueryParameters queryParameters, CancellationToken cancellationToken);

        Task<InventoryStock?> GetStockByProductAndWarehouseIDAsync(int productId, int warehouseId, CancellationToken cancellationToken);
        Task<InventoryStock?> GetStockByIdAsync(int stockId, CancellationToken cancellationToken);
        Task<InventoryStock> CreateInventoryStockAsync(InventoryStock stock, CancellationToken cancellationToken);
    }
}
