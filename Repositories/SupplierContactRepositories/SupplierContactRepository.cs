using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.SupplierContactRepositories
{
    public class SupplierContactRepository : ISupplierContactRepository
    {
        private readonly InvManDBContext _context;

        public SupplierContactRepository(InvManDBContext context) => _context = context;

        public Task<SupplierContact?> GetByIdAsync(int supplierId, int contactId, CancellationToken cancellationToken = default) =>
            _context.SupplierContacts.SingleOrDefaultAsync(
                contact => contact.SupplierID == supplierId && contact.ID == contactId,
                cancellationToken);

        public async Task<IEnumerable<SupplierContact>> GetAllBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default) =>
            await _context.SupplierContacts.AsNoTracking()
                .Where(contact => contact.SupplierID == supplierId)
                .OrderByDescending(contact => contact.IsPrimary)
                .ThenBy(contact => contact.Name)
                .ToListAsync(cancellationToken);

        public Task<SupplierContact?> GetPrimaryBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default) =>
            _context.SupplierContacts
                .FirstOrDefaultAsync(contact => contact.SupplierID == supplierId && contact.IsPrimary, cancellationToken);

        public Task<bool> SupplierExistsAsync(int supplierId, CancellationToken cancellationToken = default) =>
            _context.Suppliers.AsNoTracking().AnyAsync(supplier => supplier.ID == supplierId && supplier.IsActive, cancellationToken);

        public Task<bool> EmailExistsForSupplierAsync(int supplierId, string email, CancellationToken cancellationToken = default) =>
            _context.SupplierContacts.AsNoTracking()
                .AnyAsync(contact => contact.SupplierID == supplierId && contact.Email == email, cancellationToken);

        public Task<bool> EmailExistsForOtherContactAsync(int supplierId, int contactId, string email, CancellationToken cancellationToken = default) =>
            _context.SupplierContacts.AsNoTracking()
                .AnyAsync(contact => contact.SupplierID == supplierId && contact.ID != contactId && contact.Email == email, cancellationToken);

        public async Task<SupplierContact> AddAsync(SupplierContact contact, CancellationToken cancellationToken = default)
        {
            await _context.SupplierContacts.AddAsync(contact, cancellationToken);
            return contact;
        }
        public void Remove(SupplierContact contact) => _context.SupplierContacts.Remove(contact);
    }
}
