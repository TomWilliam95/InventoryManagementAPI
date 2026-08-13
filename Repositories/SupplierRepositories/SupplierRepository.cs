using InventoryManagementAPI.Models.CoreModels.SupplierModels;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.SupplierRepositories
{
    public class SupplierRepository : ISupplierRepository
    {
        // === CONSTRUCTOR DI ===
        private readonly InvManDBContext _context;
        public SupplierRepository(InvManDBContext context)
        {
            _context = context;
        }

        // === GET ===
        public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Supplier?> GetSupplierByIdAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.FindAsync(supplierId, cancellationToken);
        }


        // === POST ===
        public async Task<Supplier> CreateSupplierAsync(Supplier supplier, CancellationToken cancellationToken = default)
        {
            await _context.Suppliers.AddAsync(supplier, cancellationToken);
            return supplier;
        }

        // === CHECK EXISTENCE ===

        public async Task<bool> SupplierExistsAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.AnyAsync(s => s.ID == supplierId, cancellationToken);
        }

        public async Task<bool> SupplierNameExistsAsync(string supplierName, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.AnyAsync(s => s.Name == supplierName, cancellationToken);
        }

        public async Task<bool> SupplierNameExistsForOtherSupplierAsync(int supplierId, string supplierName, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.AnyAsync(s => s.Name == supplierName && s.ID != supplierId, cancellationToken);
        }

    }
}
