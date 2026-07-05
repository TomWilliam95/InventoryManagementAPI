using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositorys.ProductRepositories
{
    public class ProductRepository : IProductRepository
    {
        // === CONSTRUCTOR DI === \\
        private readonly InvManDBContext _context;
        public ProductRepository(InvManDBContext context)
        {
            _context = context;
        }

        // === GET === \\
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetProductAsync(int id)
        {
            return await _context.Products.Include(p => p.Category).
                Include(p => p.Supplier).
                FirstOrDefaultAsync(p => p.ID == id);
        }
        public async Task<IEnumerable<Product>> GetProductsByCategory(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryID == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsBelowReorderLevelAsync()
        {
            return await _context.Products.Where(p => p.QuantityInStock < p.ReorderLevel).ToListAsync();
        }

        // === POST === \\
        public async Task<Product> AddProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        // === PUT === \\
        public async Task<bool> UpdateProductDetailsAsync(int id, Product product)
        {
            var updatedProduct = await _context.Products.FindAsync(id);
            if (updatedProduct == null) return false;

            updatedProduct.Sku = product.Sku;
            updatedProduct.Name = product.Name;
            updatedProduct.Description = product.Description;
            updatedProduct.CategoryID = product.CategoryID;
            updatedProduct.SupplierID = product.SupplierID;
            updatedProduct.IsActive = product.IsActive;
            updatedProduct.Updated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        // === DELETE === \\
        public async Task<bool> RemoveProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        // === CHECK EXISTENCE === \\
        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.ID == id);
        }

        // === Save Changes === \\
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
