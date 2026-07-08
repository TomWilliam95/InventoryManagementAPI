using InventoryManagementAPI.Models.DTO_s.CategoryDTO_s;
using InventoryManagementAPI.Repositories.CategoryRepositories;
using Microsoft.AspNetCore.Authorization;
using InventoryManagementAPI.Models.CoreModels;
using Microsoft.AspNetCore.Http;
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

        // === GET === \\
        [HttpGet("Category/{id}")]
        public async Task<ActionResult<ApiResponse<SingleCategoryResponseDTO>>> GetCategory(int id)
        {
            var category = await _categoryService.GetSingleCategory(id);
            return StatusCode(category.StatusCode, category);
        }

        [HttpGet("AllCategories")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkCategoryResponseDTO>>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategories();
            return StatusCode(categories.StatusCode, categories);
        }

        
        // === POST === \\
        [HttpPost("AddCategory")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<SingleCategoryResponseDTO>>> AddCategory(CreateCategoryRequestDTO dto)
        {
            var addedCategory = await _categoryService.AddCategory(dto);
            return addedCategory.StatusCode switch
            {
                201 => CreatedAtAction(nameof(GetCategory), new { id = addedCategory.Data.ID }, addedCategory),
                _ => StatusCode(addedCategory.StatusCode, addedCategory)
            };
        }


        // === PUT === \\
        [HttpPut("UpdateCategory/{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<SingleCategoryResponseDTO>>> UpdateCategoryDetails(int id, UpdateCategoryDetailsRequestDTO dto)
        {
            var updatedCategory = await _categoryService.UpdateCategoryDetails(id, dto);
            return StatusCode(updatedCategory.StatusCode, updatedCategory);
        }

        // === SET ACTIVE STATUS === \\
        [HttpPut("ActivateCategory/{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<SingleCategoryResponseDTO>>> ActivateCategory(int id)
        {
            var activatedCategory = await _categoryService.ActivateCategory(id);
            return StatusCode(activatedCategory.StatusCode, activatedCategory);
        }

        [HttpPut("DeactivateCategory/{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<SingleCategoryResponseDTO>>> DeactivateCategory(int id)
        {
            var deactivatedCategory = await _categoryService.DeactivateCategory(id);
            return StatusCode(deactivatedCategory.StatusCode, deactivatedCategory);
        }
    }
}
