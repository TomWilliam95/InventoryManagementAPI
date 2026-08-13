namespace InventoryManagementAPI.Repositories.SupplierProductRepositories
{
    public interface ISupplierProductRepository
    {
        Task<SupplierProduct?> GetAsync(int supplierId, int productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<SupplierProduct>> GetAllBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<IEnumerable<SupplierProduct>> GetAllByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<SupplierProduct?> GetPreferredSupplierForProductAsync(int productId, CancellationToken cancellationToken = default);
        Task<bool> AssignmentExistsAsync(int supplierId, int productId, CancellationToken cancellationToken = default);
        Task<bool> SupplierSkuExistsAsync(int supplierId, string supplierSku, CancellationToken cancellationToken = default);
        Task<bool> SupplierSkuExistsForOtherProductAsync(int supplierId, int productId, string supplierSku, CancellationToken cancellationToken = default);
        Task<bool> SupplierExistsAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken = default);
        Task AddAsync(SupplierProduct supplierProduct, CancellationToken cancellationToken = default);
        void Remove(SupplierProduct supplierProduct);
    }
}
