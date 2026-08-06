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
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier).ToListAsync();
        }

        public async Task<Product?> GetProductAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ID == id);
        }
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.CategoryID == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsBelowReorderLevelAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.QuantityInStock < p.ReorderLevel)
                .ToListAsync();
        }

        // === POST === \\
        public async Task<Product> AddProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            return product;
        }

        // === CHECK EXISTENCE === \\
        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.ID == id);
        }

        public async Task<bool> OtherProductNameExistsAsync(int id, string name)
        {
            return await _context.Products.AnyAsync(p => p.Name == name && p.ID != id);
        }
        public async Task<bool> ProductNameExistsAsync(string name)
        {
            return await _context.Products.AnyAsync(p => p.Name == name);
        }

        public async Task<bool> OtherProductSkuExistsAsync(int id, string sku)
        {
            return await _context.Products.AnyAsync(p => p.Sku == sku && p.ID != id);
        }
        public async Task<bool> ProductSkuExistsAsync(string sku)
        {
            return await _context.Products.AnyAsync(p => p.Sku == sku);
        }

        // === CHECK ACTIVE STATUS === \\
        public async Task<bool> IsProductActiveAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            return product?.IsActive == true;
        }

        // === Save Changes === \\
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
