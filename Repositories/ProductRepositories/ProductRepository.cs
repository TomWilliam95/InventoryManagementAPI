using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositorys.ProductRepositories
{
    public class ProductRepository : IProductRepository
    {
        // === CONSTRUCTOR DI ===
        private readonly InvManDBContext _context;
        public ProductRepository(InvManDBContext context)
        {
            _context = context;
        }

        // === GET ===
        public async Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.InventoryStocks)
                    .ThenInclude(stock => stock.Warehouse)
                .Include(p => p.SupplierProducts)
                    .ThenInclude(sp => sp.Supplier)
                .ToListAsync(cancellationToken);
        }

        public async Task<Product?> GetProductAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.InventoryStocks)
                    .ThenInclude(stock => stock.Warehouse)
                .Include(p => p.SupplierProducts)
                    .ThenInclude(sp => sp.Supplier)
                .SingleOrDefaultAsync(p => p.ID == id, cancellationToken);
        }
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.InventoryStocks)
                    .ThenInclude(stock => stock.Warehouse)
                .Include(p => p.SupplierProducts)
                    .ThenInclude(sp => sp.Supplier)
                .Where(p => p.CategoryID == categoryId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetProductsBelowReorderLevelAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.InventoryStocks)
                    .ThenInclude(stock => stock.Warehouse)
                .Include(p => p.SupplierProducts)
                    .ThenInclude(sp => sp.Supplier)
                .Where(p => p.InventoryStocks.Any(stock => stock.Quantity < stock.ReorderLevel))
                .ToListAsync(cancellationToken);
        }

        // === POST ===
        public async Task<Product> AddProductAsync(Product product, CancellationToken cancellationToken = default)
        {
            await _context.Products.AddAsync(product, cancellationToken);
            return product;
        }

        // === CHECK EXISTENCE ===
        public async Task<bool> ProductExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Products.AnyAsync(p => p.ID == id, cancellationToken);
        }

        public async Task<bool> OtherProductNameExistsAsync(int id, string name, CancellationToken cancellationToken = default)
        {
            return await _context.Products.AnyAsync(p => p.Name == name && p.ID != id, cancellationToken);
        }
        public async Task<bool> ProductNameExistsAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _context.Products.AnyAsync(p => p.Name == name, cancellationToken);
        }

        public async Task<bool> OtherProductSkuExistsAsync(int id, string sku, CancellationToken cancellationToken = default)
        {
            return await _context.Products.AnyAsync(p => p.Sku == sku && p.ID != id, cancellationToken);
        }
        public async Task<bool> ProductSkuExistsAsync(string sku, CancellationToken cancellationToken = default)
        {
            return await _context.Products.AnyAsync(p => p.Sku == sku, cancellationToken);
        }

        // === CHECK ACTIVE STATUS ===
        public async Task<bool> IsProductActiveAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _context.Products.FindAsync(id, cancellationToken);
            return product?.IsActive == true;
        }
    }
}
