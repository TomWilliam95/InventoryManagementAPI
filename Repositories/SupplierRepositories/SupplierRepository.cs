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
            _context.Suppliers.Update(updatedSupplierData);
            await _context.SaveChangesAsync();
        }

        // === CHECK EXISTENCE === \\

        public async Task<bool> SupplierExistsAsync(int supplierId)
        {
            return await _context.Suppliers.AnyAsync(s => s.ID == supplierId);
        }

        public async Task<bool> SupplierNameExistsAsync(string supplierName)
        {
            return await _context.Suppliers.AnyAsync(s => s.Name == supplierName);
        }

        public async Task<bool> SupplierNameExistsForOtherSupplierAsync(int supplierId, string supplierName)
        {
            return await _context.Suppliers.AnyAsync(s => s.Name == supplierName && s.ID != supplierId);
        }

        public async Task<bool> SupplierEmailExistsAsync(string supplierEmail)
        {
            return await _context.Suppliers.AnyAsync(s => s.EmailContact == supplierEmail);
        }

        public async Task<bool> SupplierEmailExistsForOtherSupplierAsync(int supplierId, string supplierEmail)
        {
            return await _context.Suppliers.AnyAsync(s => s.EmailContact == supplierEmail && s.ID != supplierId);
        }

        // === SAVE CHANGES === \\
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
