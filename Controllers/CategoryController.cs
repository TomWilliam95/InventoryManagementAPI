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

        // === GET === \\
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<SingleCategoryResponseDTO>>> GetCategory(int id)
        {
            var category = await _categoryService.GetSingleCategory(id);
            return StatusCode(category.StatusCode, category);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkCategoryResponseDTO>>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategories();
            return StatusCode(categories.StatusCode, categories);
        }

        
        // === POST === \\
        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<SingleCategoryResponseDTO>>> AddCategory(CreateCategoryRequestDTO dto)
        {
            var addedCategory = await _categoryService.AddCategory(dto);
            return addedCategory.StatusCode switch
            {
                201 when addedCategory.Data is not null => 
                CreatedAtAction(nameof(GetCategory), new { id = addedCategory.Data!.ID }, addedCategory),
                
                _ => StatusCode(addedCategory.StatusCode, addedCategory)
            };
        }


        // === PUT === \\
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<SingleCategoryResponseDTO>>> UpdateCategoryDetails(int id, UpdateCategoryDetailsRequestDTO dto)
        {
            var updatedCategory = await _categoryService.UpdateCategoryDetails(id, dto);
            return StatusCode(updatedCategory.StatusCode, updatedCategory);
        }

        // === SET ACTIVE STATUS === \\
        [HttpPatch("{id:int}/activate")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<SingleCategoryResponseDTO>>> ActivateCategory(int id)
        {
            var activatedCategory = await _categoryService.ActivateCategory(id);
            return StatusCode(activatedCategory.StatusCode, activatedCategory);
        }

        [HttpPatch("{id:int}/deactivate")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<SingleCategoryResponseDTO>>> DeactivateCategory(int id)
        {
            var deactivatedCategory = await _categoryService.DeactivateCategory(id);
            return StatusCode(deactivatedCategory.StatusCode, deactivatedCategory);
        }
    }
}
