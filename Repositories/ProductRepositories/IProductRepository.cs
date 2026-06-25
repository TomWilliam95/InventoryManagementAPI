using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;

namespace InventoryManagementAPI.Repositories.ProductRepositorys
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductAsync(int id);
        Task<IEnumerable<Product>> GetProductsByCategory(int categoryId);
        Task<Product> AddProductAsync(Product product);
        Task<bool> UpdateProductDetailsAsync(int id, Product product);
        Task<bool> UpdateProductPriceAsync(int id, decimal newProductPrice);
        Task<bool> UpdateProductStockQuantityAsync(int id, int newStockQuantity);
        Task<bool> UpdateProductReorderLevelAsync(int id, int newReorderLevel);
        Task<IEnumerable<Product>> GetProductsBelowReorderLevelAsync();
        Task<bool> RemoveProductAsync(int id);
        Task<bool> ProductExistsAsync(int id);
    }
}
