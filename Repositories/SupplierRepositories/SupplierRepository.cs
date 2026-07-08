using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.SupplierRepositories
{
    public class SupplierRepository : ISupplierRepository
    {
        // === CONSTRUCTOR DI === \\
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

        public async Task<Supplier?> GetSupplierByIdAsync(int supplierId)
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

        // === PUT === \\
        public async Task UpdateSupplierAsync(Supplier updatedSupplierData)
        {
            var supplierUpdating = await _context.Suppliers.FindAsync(updatedSupplierData.ID);

            if (supplierUpdating == null) return;

            supplierUpdating.Name = updatedSupplierData.Name;
            supplierUpdating.ContactName = updatedSupplierData.ContactName;
            supplierUpdating.PhoneContact = updatedSupplierData.PhoneContact;
            supplierUpdating.EmailContact = updatedSupplierData.EmailContact;
            supplierUpdating.Address = updatedSupplierData.Address;
            supplierUpdating.LastUpdated = DateOnly.FromDateTime(DateTime.Now);
            await _context.SaveChangesAsync();
        }

        // === CHECK EXISTENCE === \\

        public async Task<bool> SupplierExistsAsync(int supplierId)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null) return false;
            return true;
        }

        // === SAVE CHANGES === \\
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
