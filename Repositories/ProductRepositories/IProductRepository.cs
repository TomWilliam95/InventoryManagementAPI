using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;

namespace InventoryManagementAPI.Repositories.ProductRepositorys
{
    public interface IProductRepository
    {
        // === GET === \\
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductAsync(int id);
        Task<IEnumerable<Product>> GetProductsByCategory(int categoryId);
        Task<IEnumerable<Product>> GetProductsBelowReorderLevelAsync();

        // === POST === \\
        Task<Product> AddProductAsync(Product product);

        // === PUT === \\
        Task<bool> UpdateProductDetailsAsync(int id, Product product);

        // === CHECK EXISTENCE === \\
        Task<bool> ProductExistsAsync(int id);
        Task<bool> UpdateProductNameExistsAsync(int id, string name);
        Task<bool> AddProductNameExistsAsync(string name);
        Task<bool> UpdateProductSkuExistsAsync(int id, string sku);
        Task<bool> AddProductSkuExistsAsync(string sku);

        // === CHECK ACTIVE STATUS === \\
        Task<bool> IsProductActiveAsync(int id);

        // === Save Changes === \\
        Task SaveChangesAsync();
    }
}
