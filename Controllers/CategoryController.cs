using InventoryManagementAPI.Models.DTO_s.CategoryDTO_s;
using InventoryManagementAPI.Repositories.CategoryRepositories;
using Microsoft.AspNetCore.Authorization;
using InventoryManagementAPI.Models.CoreModels;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementAPI.Controllers
{
    [Route("api/categories")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // === GET ===
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<SingleCategoryResponseDTO>>> GetCategory(int id, CancellationToken cancellationToken = default)
        {
            var category = await _categoryService.GetSingleCategory(id, cancellationToken);
            return StatusCode(category.StatusCode, category);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkCategoryResponseDTO>>>> GetAllCategories(CancellationToken cancellationToken = default)
        {
            var categories = await _categoryService.GetAllCategories(cancellationToken);
            return StatusCode(categories.StatusCode, categories);
        }

        
        // === POST ===
        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<SingleCategoryResponseDTO>>> AddCategory(CreateCategoryRequestDTO dto, CancellationToken cancellationToken = default)
        {
            var addedCategory = await _categoryService.AddCategory(dto, cancellationToken);
            return addedCategory.StatusCode switch
            {
                201 when addedCategory.Data is not null =>
                CreatedAtAction(nameof(GetCategory), new { id = addedCategory.Data!.ID }, addedCategory),

                _ => StatusCode(addedCategory.StatusCode, addedCategory)
            };
        }


        // === PUT ===
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<SingleCategoryResponseDTO>>> UpdateCategoryDetails(int id, UpdateCategoryDetailsRequestDTO dto, CancellationToken cancellationToken = default)
        {
            var updatedCategory = await _categoryService.UpdateCategoryDetails(id, dto, cancellationToken);
            return StatusCode(updatedCategory.StatusCode, updatedCategory);
        }

        // === SET ACTIVE STATUS ===
        [HttpPatch("{id:int}/activate")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<SingleCategoryResponseDTO>>> ActivateCategory(int id, UpdateCategoryStatusRequestDTO dto, CancellationToken cancellationToken = default)
        {
            var activatedCategory = await _categoryService.ActivateCategory(id, dto, cancellationToken);
            return StatusCode(activatedCategory.StatusCode, activatedCategory);
        }

        [HttpPatch("{id:int}/deactivate")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<SingleCategoryResponseDTO>>> DeactivateCategory(int id, UpdateCategoryStatusRequestDTO dto, CancellationToken cancellationToken = default)
        {
            var deactivatedCategory = await _categoryService.DeactivateCategory(id, dto, cancellationToken);
            return StatusCode(deactivatedCategory.StatusCode, deactivatedCategory);
        }
    }
}
