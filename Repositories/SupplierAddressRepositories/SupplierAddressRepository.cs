using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.SupplierAddressRepositories
{
    public class SupplierAddressRepository : ISupplierAddressRepository
    {
        private readonly InvManDBContext _context;

        public SupplierAddressRepository(InvManDBContext context) => _context = context;

        public Task<SupplierAddress?> GetByIdAsync(int supplierId, int addressId, CancellationToken cancellationToken = default) =>
 _context.SupplierAddresses.SingleOrDefaultAsync(address => address.SupplierID == supplierId && address.ID == addressId, cancellationToken);

        public async Task<IEnumerable<SupplierAddress>> GetAllBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default) =>
            await _context.SupplierAddresses.AsNoTracking()
                .Where(address => address.SupplierID == supplierId)
                .OrderByDescending(address => address.IsPrimary)
                .ThenBy(address => address.Type)
                .ToListAsync(cancellationToken);

        public async Task<IEnumerable<SupplierAddress>> GetByTypeAsync(int supplierId, SupplierAddressType type, CancellationToken cancellationToken = default) =>
            await _context.SupplierAddresses.AsNoTracking()
                .Where(address => address.SupplierID == supplierId && address.Type == type)
                .OrderByDescending(address => address.IsPrimary)
                .ToListAsync(cancellationToken);

        public Task<SupplierAddress?> GetPrimaryByTypeAsync(int supplierId, SupplierAddressType type, CancellationToken cancellationToken = default) =>
 _context.SupplierAddresses.FirstOrDefaultAsync(address => address.SupplierID == supplierId && address.Type == type && address.IsPrimary, cancellationToken);

        public Task<bool> ExistsAsync(int supplierId, int addressId, CancellationToken cancellationToken = default) =>
            _context.SupplierAddresses.AsNoTracking()
                .AnyAsync(address => address.SupplierID == supplierId && address.ID == addressId, cancellationToken);

        public Task<bool> SupplierExistsAsync(int supplierId, CancellationToken cancellationToken = default) =>
            _context.Suppliers.AsNoTracking().AnyAsync(supplier => supplier.ID == supplierId && supplier.IsActive, cancellationToken);

        public async Task AddAsync(SupplierAddress address, CancellationToken cancellationToken = default) =>
            await _context.SupplierAddresses.AddAsync(address, cancellationToken);
    }
}
