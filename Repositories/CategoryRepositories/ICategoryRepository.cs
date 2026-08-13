using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Repositories.CategoryRepositories
{
    public interface ICategoryRepository
    {
        // === GET ===
        Task<IEnumerable<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
        Task<Category?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default);

        // === POST ===
        Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default);

        // === PUT ===
        Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default);

        // === CHECK EXISTENCE ===
        Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default);
        Task<bool> OtherCategoryNameExistsAsync(int categoryId, string categoryName, CancellationToken cancellationToken = default);
        Task<bool> CategoryNameExistsASync(string categoryName, CancellationToken cancellationToken = default);

        // === SAVE CHANGES ===
    }
}
