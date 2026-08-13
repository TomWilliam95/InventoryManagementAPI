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

        // === GET ===
        public async Task<Category?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Categories.FindAsync(categoryId, cancellationToken);
        }
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        // === POST ===
        public async Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default)
        {
            await _context.Categories.AddAsync(category, cancellationToken);
            return category;
        }

        // === PUT ===
        public Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default)
        {
            _context.Categories.Update(category);
            return Task.CompletedTask;
        }

        // === CHECK EXISTENCE ===
        public async Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Categories.AnyAsync(c => c.ID == categoryId, cancellationToken);
        }

        public async Task<bool> OtherCategoryNameExistsAsync(int categoryId, string categoryName, CancellationToken cancellationToken = default)
        {
            return await _context.Categories.AnyAsync(c => c.Name == categoryName && c.ID != categoryId, cancellationToken);
        }
        public async Task<bool> CategoryNameExistsASync(string categoryName, CancellationToken cancellationToken = default)
        {
            return await _context.Categories.AnyAsync(c => c.Name == categoryName, cancellationToken);
        }

    }
}
