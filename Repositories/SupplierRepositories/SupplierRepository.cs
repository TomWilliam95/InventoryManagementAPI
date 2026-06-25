using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.SupplierRepositories
{
    public class SupplierRepository : ISupplierRepository
    {
        // === || CONSTRUCTER DI || === \\
        private readonly InvManDBContext _context;
        public SupplierRepository(InvManDBContext context)
        {
            _context = context;
        }

        // === GET === \\
        public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
        {
            return await _context.Suppliers.ToListAsync();
        }

        public async Task<Supplier> GetSupplierByIdAsync(int supplierId)
        {
            return await _context.Suppliers.FindAsync(supplierId);
        }


        // === POST === \\
        public async Task<Supplier> CreateSupplierAsync(Supplier supplier)
        {
            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        // === PUT === \
        public async Task<bool> UpdateSupplierAsync(int supplierId, Supplier updatedSupplier)
        {
            var findSupplier = await _context.Suppliers.FindAsync(supplierId);

            if (findSupplier == null) return false;

            findSupplier.Name = updatedSupplier.Name;
            findSupplier.ContactName = updatedSupplier.ContactName;
            findSupplier.PhoneContact = updatedSupplier.PhoneContact;
            findSupplier.EmailContact = updatedSupplier.EmailContact;
            findSupplier.Address = updatedSupplier.Address;
            findSupplier.IsActive = updatedSupplier.IsActive;
            updatedSupplier.LastUpdated = DateOnly.FromDateTime(DateTime.Now);
            await _context.SaveChangesAsync();
            return true;
        }


        // === DELETE === \\
        public async Task<bool> DeleteSupplierAsync(int supplierId)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);

            if (supplier == null) return false;

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return true;
        }


        // === CHECK IF EXISTS === \\
        public async Task<bool> SupplierExistsAsync(int supplierId)
        {
            return await _context.Suppliers.AnyAsync(s => s.ID == supplierId);
        }
    }
}
