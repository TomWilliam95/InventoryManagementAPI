using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;

namespace InventoryManagementAPI.Repositories.ProductRepositorys
{
    public interface IProductRepository
    {
        // === GET ===
        Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default);
        Task<Product?> GetProductAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> GetProductsBelowReorderLevelAsync(CancellationToken cancellationToken = default);

        // === POST ===
        Task<Product> AddProductAsync(Product product, CancellationToken cancellationToken = default);

        // === CHECK EXISTENCE ===
        Task<bool> ProductExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> OtherProductNameExistsAsync(int id, string name, CancellationToken cancellationToken = default);
        Task<bool> ProductNameExistsAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> OtherProductSkuExistsAsync(int id, string sku, CancellationToken cancellationToken = default);
        Task<bool> ProductSkuExistsAsync(string sku, CancellationToken cancellationToken = default);

        // === CHECK ACTIVE STATUS ===
        Task<bool> IsProductActiveAsync(int id, CancellationToken cancellationToken = default);

        // === Save Changes ===
    }
}
