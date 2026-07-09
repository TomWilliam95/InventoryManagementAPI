using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Repositories.CategoryRepositories
{
    public interface ICategoryRepository
    {
        // === GET === \\
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int categoryId);

        // === POST === \\
        Task<Category> CreateCategoryAsync(Category category);

        // === PUT === \\
        Task UpdateCategoryAsync(int categoryId, Category category);

        // === CHECK EXISTENCE === \\
        Task<bool> CategoryExistsAsync(int categoryId);
        Task<bool> UpdateCategoryNameExistsAsync(int categoryId, string categoryName);
        Task<bool> AddCategoryNameExistsASync(string categoryName);

        // === SAVE CHANGES === \\
        Task SaveChangesAsync();
    }
}
