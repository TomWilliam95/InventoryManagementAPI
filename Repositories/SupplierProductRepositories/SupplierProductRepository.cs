using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.SupplierProductRepositories
{
    public class SupplierProductRepository : ISupplierProductRepository
    {
        private readonly InvManDBContext _context;

        public SupplierProductRepository(InvManDBContext context) => _context = context;

        public Task<SupplierProduct?> GetAsync(int supplierId, int productId, CancellationToken cancellationToken = default) =>
            _context.SupplierProducts
                .Include(item => item.Supplier)
                .Include(item => item.Product)
                .SingleOrDefaultAsync(item => item.SupplierID == supplierId && item.ProductID == productId, cancellationToken);

        public async Task<IEnumerable<SupplierProduct>> GetAllBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default) =>
            await _context.SupplierProducts.AsNoTracking()
                .Include(item => item.Product)
                .Where(item => item.SupplierID == supplierId)
                .OrderByDescending(item => item.IsPreferred)
                .ThenBy(item => item.Product.Name)
                .ToListAsync(cancellationToken);

        public async Task<IEnumerable<SupplierProduct>> GetAllByProductIdAsync(int productId, CancellationToken cancellationToken = default) =>
            await _context.SupplierProducts.AsNoTracking()
                .Include(item => item.Supplier)
                .Where(item => item.ProductID == productId)
                .OrderByDescending(item => item.IsPreferred)
                .ThenBy(item => item.UnitCost)
                .ToListAsync(cancellationToken);

        public Task<SupplierProduct?> GetPreferredSupplierForProductAsync(int productId, CancellationToken cancellationToken = default) =>
            _context.SupplierProducts
                .Include(item => item.Supplier)
                .FirstOrDefaultAsync(item => item.ProductID == productId && item.IsPreferred && item.IsActive, cancellationToken);

        public Task<bool> AssignmentExistsAsync(int supplierId, int productId, CancellationToken cancellationToken = default) =>
            _context.SupplierProducts.AsNoTracking()
                .AnyAsync(item => item.SupplierID == supplierId && item.ProductID == productId, cancellationToken);

        public Task<bool> SupplierSkuExistsAsync(int supplierId, string supplierSku, CancellationToken cancellationToken = default) =>
            _context.SupplierProducts.AsNoTracking()
                .AnyAsync(item => item.SupplierID == supplierId && item.SupplierSku == supplierSku, cancellationToken);

        public Task<bool> SupplierSkuExistsForOtherProductAsync(int supplierId, int productId, string supplierSku, CancellationToken cancellationToken = default) =>
 _context.SupplierProducts.AsNoTracking().AnyAsync(item => item.SupplierID == supplierId && item.ProductID != productId && item.SupplierSku == supplierSku, cancellationToken);

        public Task<bool> SupplierExistsAsync(int supplierId, CancellationToken cancellationToken = default) =>
            _context.Suppliers.AsNoTracking().AnyAsync(supplier => supplier.ID == supplierId && supplier.IsActive, cancellationToken);

        public Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken = default) =>
            _context.Products.AsNoTracking().AnyAsync(product => product.ID == productId && product.IsActive, cancellationToken);

        public async Task AddAsync(SupplierProduct supplierProduct, CancellationToken cancellationToken = default) =>
            await _context.SupplierProducts.AddAsync(supplierProduct, cancellationToken);

        public void Remove(SupplierProduct supplierProduct) => _context.SupplierProducts.Remove(supplierProduct);
    }
}
