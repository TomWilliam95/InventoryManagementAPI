using InventoryManagementAPI.Models.Contracts.Products;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.Shared;
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
        public async Task<PagedData<Product>> GetProductsAsync(ProductQueryParameters query, CancellationToken cancellationToken = default)
        {
            var products = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.InventoryStocks)
                    .ThenInclude(stock => stock.Warehouse)
                .Include(p => p.SupplierProducts)
                    .ThenInclude(sp => sp.Supplier).AsQueryable();

            if(!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();

                products = products.Where(p => p.Name.ToLower().Contains(search) || p.Sku.ToLower().Contains(search));
            }
            if(query.CategoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == query.CategoryId.Value);
            }
            if(query.IsActive.HasValue)
            {
                products = products.Where(p => p.IsActive == query.IsActive.Value);
            }
            if(query.MinPrice.HasValue)
            {
                products = products.Where(p => p.Price >= query.MinPrice.Value);
            }
            if (query.MaxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= query.MaxPrice.Value);
            }

            //Count total items before pagination
            var totalItems = await products.CountAsync(cancellationToken);
            
            // Sorting
            var sortBy = query.SortBy.Trim().ToLowerInvariant();
            var descending = query.SortDirection.Trim().ToLowerInvariant() == "desc";

            products = sortBy switch
            {
                "price" when descending => products.OrderByDescending(p => p.Price).ThenBy(p => p.ID),
                "price" => products.OrderBy(p => p.Price).ThenBy(p => p.ID),
                "sku" when descending => products.OrderByDescending(p => p.Sku).ThenBy(p => p.ID),
                "sku" => products.OrderBy(p => p.Sku).ThenBy(p => p.ID),
                "name" when descending => products.OrderByDescending(p => p.Name).ThenBy(p => p.ID),
                "name" => products.OrderBy(p => p.Name).ThenBy(p => p.ID),
                "created" when descending => products.OrderByDescending(p => p.Created).ThenBy(p => p.ID),
                "created" => products.OrderBy(p => p.Created).ThenBy(p => p.ID),
                "id" when descending => products.OrderByDescending(p => p.ID),
                _ => products.OrderBy(p => p.ID),
            };

            var recordsToSkip = (query.Page - 1) * query.PageSize;
            var productList = await products.Skip(recordsToSkip).Take(query.PageSize).ToListAsync(cancellationToken);

            return new PagedData<Product>
            {
                Items = productList,
                TotalItems = totalItems
            };
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
