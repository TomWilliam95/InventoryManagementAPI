using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.CategoryDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PATCH;

namespace InventoryManagementAPI.Repositories.CategoryRepositories
{
    public interface ICategoryService
    {
        // === GET ===
        Task<ApiResponse<SingleCategoryResponseDTO>> GetSingleCategory(int categoryId);
        Task<ApiResponse<IEnumerable<BulkCategoryResponseDTO>>> GetAllCategories();

        // === POST ===
        Task<ApiResponse<SingleCategoryResponseDTO>> AddCategory(CreateCategoryRequestDTO dto);

        // === PUT ===
        Task<ApiResponse<SingleCategoryResponseDTO>> UpdateCategoryDetails(int id, UpdateCategoryDetailsRequestDTO dto);

        // === SET ACTIVE STATUS ===
        Task<ApiResponse<SingleCategoryResponseDTO>> ActivateCategory(int id, UpdateCategoryStatusRequestDTO dto);
        Task<ApiResponse<SingleCategoryResponseDTO>> DeactivateCategory(int id, UpdateCategoryStatusRequestDTO dto);
    }
}
