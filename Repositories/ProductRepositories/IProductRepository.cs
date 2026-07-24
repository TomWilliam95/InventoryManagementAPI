using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;

namespace InventoryManagementAPI.Repositories.ProductRepositorys
{
    public interface IProductRepository
    {
        // === GET === \\
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductAsync(int id);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetProductsBelowReorderLevelAsync();

        // === POST === \\
        Task<Product> AddProductAsync(Product product);

        // === CHECK EXISTENCE === \\
        Task<bool> ProductExistsAsync(int id);
        Task<bool> OtherProductNameExistsAsync(int id, string name);
        Task<bool> ProductNameExistsAsync(string name);
        Task<bool> OtherProductSkuExistsAsync(int id, string sku);
        Task<bool> ProductSkuExistsAsync(string sku);

        // === CHECK ACTIVE STATUS === \\
        Task<bool> IsProductActiveAsync(int id);

        // === Save Changes === \\
        Task SaveChangesAsync();
    }
}
