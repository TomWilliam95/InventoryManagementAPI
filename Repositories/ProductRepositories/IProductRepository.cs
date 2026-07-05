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

        // === DELETE === \\
        Task<bool> RemoveProductAsync(int id);

        // === CHECK EXISTENCE === \\
        Task<bool> ProductExistsAsync(int id);

        // === Save Changes === \\
        Task SaveChangesAsync();
    }
}
