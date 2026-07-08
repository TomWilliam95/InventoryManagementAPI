using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Repositories.CategoryRepositories
{
    public interface ICategoryRepository
    {
        // === GET === \\
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int categoryId);

        // === POST === \\
        Task<Category> CreateCategoryAsync(Category category);

        // === PUT === \\
        Task<bool> UpdateCategoryAsync(int categoryId, Category category);

        // === CHECK EXISTENCE === \\
        Task<bool> CategoryExistsAsync(int categoryId);

        // === SAVE CHANGES === \\
        Task SaveChangesAsync();
    }
}
