using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.CategoryRepositories
{
    public class CategoryRepository: ICategoryRepository
    {
        private readonly InvManDBContext _context;
        public CategoryRepository(InvManDBContext context)
        {
            _context = context;
        }

        // === GET === \\
        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories.FindAsync(categoryId);
        }
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        // === POST === \\
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        // === PUT === \\
        public async Task UpdateCategoryAsync(int categoryId, Category category)
        {
            var updatingCategory = await _context.Categories.FindAsync(categoryId);
            if (updatingCategory == null) return;

            updatingCategory.Name = category.Name;
            updatingCategory.Description = category.Description;
            updatingCategory.Updated = DateOnly.FromDateTime(DateTime.Now);
            
            await _context.SaveChangesAsync();
        }

        // === CHECK EXISTENCE === \\
        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.ID == categoryId);
        }

        public async Task<bool> UpdateCategoryNameExistsAsync(int categoryId, string categoryName)
        {
            return await _context.Categories.AnyAsync(c => c.Name == categoryName && c.ID != categoryId);
        }
        public async Task<bool> AddCategoryNameExistsASync(string categoryName)
        {
            return await _context.Categories.AnyAsync(c => c.Name == categoryName);
        }

        // === SAVE CHANGES === \\
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
