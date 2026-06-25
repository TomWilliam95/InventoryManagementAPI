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

        // === POST === \\
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }



        // === GET === \\
        public async Task<Category> GetCategoryByIdAsync(int categoryId)
        {
            return await  _context.Categories.FindAsync(categoryId);
        }
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }


        // === PUT === \\
        public async Task<bool> UpdateCategoryAsync(int categoryId, Category category)
        {
            var updatingCategory = await _context.Categories.FindAsync(categoryId);
            if (updatingCategory == null) return false;

            updatingCategory.Name = category.Name;
            updatingCategory.Description = category.Description;
            updatingCategory.IsActive = category.IsActive;
            updatingCategory.Updated = DateOnly.FromDateTime(DateTime.Now);
            
            await _context.SaveChangesAsync();
            return true;
        }


        // === DELETE === \\
        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }


        // === CHECK EXISTENCE === \\
        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.ID == categoryId);
        }
    }
}
