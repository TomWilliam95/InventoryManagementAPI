using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.CategoryDTO_s;

namespace InventoryManagementAPI.Repositories.CategoryRepositories
{
    public interface ICategoryService
    {
        // === GET === \\
        Task<ApiResponse<SingleCategoryResponseDTO>> GetSingleCategory(int categoryId);
        Task<ApiResponse<IEnumerable<BulkCategoryResponseDTO>>> GetAllCategories();

        // === POST === \\
        Task<ApiResponse<SingleCategoryResponseDTO>> AddCategory(CreateCategoryRequestDTO dto);

        // === PUT === \\
        Task<ApiResponse<SingleCategoryResponseDTO>> UpdateCategoryDetails(int id, UpdateCategoryDetailsRequestDTO dto);

        // === DELETE === \\
        Task<ApiResponse<object>> DeleteCategory(int id);
    }
}
