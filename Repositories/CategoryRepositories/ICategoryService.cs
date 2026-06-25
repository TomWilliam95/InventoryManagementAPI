using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.CategoryDTO_s;

namespace InventoryManagementAPI.Repositories.CategoryRepositories
{
    public interface ICategoryService
    {
            Task<ApiResponse<SingleCategoryResponseDTO>> AddCategory(CreateCategoryRequestDTO dto);
            Task<ApiResponse<SingleCategoryResponseDTO>> GetSingleCategory(int categoryId);
            Task<ApiResponse<IEnumerable<BulkCategoryResponseDTO>>> GetAllCategories();
            Task<ApiResponse<object>> DeleteCategory(int id);
            Task<ApiResponse<SingleCategoryResponseDTO>> UpdateCategoryDetails(int id, UpdateCategoryDetailsRequestDTO dto);
    }
}
