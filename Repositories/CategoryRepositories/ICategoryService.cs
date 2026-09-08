using InventoryManagementAPI.Models.Contracts.Categories;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.CategoryDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PATCH;
using InventoryManagementAPI.Models.Shared;

namespace InventoryManagementAPI.Repositories.CategoryRepositories
{
    public interface ICategoryService
    {
        // === GET ===
        Task<ApiResponse<SingleCategoryResponseDTO>> GetSingleCategory(int categoryId, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<BulkCategoryResponseDTO>>> GetCategories(CategoryQueryParameters query, CancellationToken cancellationToken = default);

        // === POST ===
        Task<ApiResponse<SingleCategoryResponseDTO>> AddCategory(CreateCategoryRequestDTO dto, CancellationToken cancellationToken = default);

        // === PUT ===
        Task<ApiResponse<SingleCategoryResponseDTO>> UpdateCategoryDetails(int id, UpdateCategoryDetailsRequestDTO dto, CancellationToken cancellationToken = default);

        // === SET ACTIVE STATUS ===
        Task<ApiResponse<SingleCategoryResponseDTO>> ActivateCategory(int id, UpdateCategoryStatusRequestDTO dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<SingleCategoryResponseDTO>> DeactivateCategory(int id, UpdateCategoryStatusRequestDTO dto, CancellationToken cancellationToken = default);
    }
}
