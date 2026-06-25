using InventoryManagementAPI.Models.CoreModels;

namespace InventoryManagementAPI.Repositories.CategoryRepositories
{
    public interface ICategoryRepository
    {
        Task<bool> CategoryExistsAsync(int categoryId);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int categoryId);
        Task<Category> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(int categoryId, Category category);
        Task<bool> DeleteCategoryAsync(int categoryId);
    }
}
