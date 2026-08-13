using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Repositories.WarehouseRepositories
{
    public interface IWarehouseRepository
    {
        public Task<Warehouse?> GetWarehouseByIdAsync(int id, CancellationToken cancellationToken = default);
        public Task<IEnumerable<Warehouse>> GetAllWarehousesAsync(CancellationToken cancellationToken = default);

        public Task<Warehouse> AddWarehouseAsync(Warehouse warehouse, CancellationToken cancellationToken = default);

        public Task<bool> IsWarehouseActiveAsync(int warehouseId, CancellationToken cancellationToken = default);
    }
}
