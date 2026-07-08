using InventoryManagementAPI.Models.DTO_s.CategoryDTO_s;
using InventoryManagementAPI.Repositories.CategoryRepositories;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<ActionResult<SingleCategoryResponseDTO>> GetCategory(int id)
        {
            var category = await _categoryService.GetSingleCategory(id);
            return category.StatusCode switch
            {
                200 => Ok(category.Data),
                404 => NotFound(category.Message),
                500 => StatusCode(500, category.Message),
                _ => StatusCode(category.StatusCode, category)
            };
        }

        [HttpGet("AllCategories")]
        public async Task<ActionResult<IEnumerable<BulkCategoryResponseDTO>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategories();
            return categories.StatusCode switch
            {
                200 => Ok(categories.Data),
                404 => NotFound(categories.Message),
                500 => StatusCode(500, categories.Message),
                _ => StatusCode(categories.StatusCode, categories)
            };
        }


        // === POST === \\
        [HttpPost("AddCategory")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<SingleCategoryResponseDTO>> AddCategory(CreateCategoryRequestDTO dto)
        {
            var addedCategory = await _categoryService.AddCategory(dto);
            return addedCategory.StatusCode switch
            {
                201 => CreatedAtAction(nameof(GetCategory), new { id = addedCategory.Data.ID }, addedCategory),
                400 => BadRequest(addedCategory.Message),
                404 => NotFound(addedCategory.Message),
                500 => StatusCode(500, addedCategory.Message),
                _ => StatusCode(addedCategory.StatusCode, addedCategory)
            };
        }


        // === PUT === \\
        [HttpPut("UpdateCategory/{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<SingleCategoryResponseDTO>> UpdateCategoryDetails(int id, UpdateCategoryDetailsRequestDTO dto)
        {
            var updatedCategory = await _categoryService.UpdateCategoryDetails(id, dto);
            return updatedCategory.StatusCode switch
            {
                200 => Ok(updatedCategory.Data),
                400 => BadRequest(updatedCategory.Message),
                404 => NotFound(updatedCategory.Message),
                500 => StatusCode(500, updatedCategory.Message),
                _ => StatusCode(updatedCategory.StatusCode, updatedCategory)
            };
        }

        // === SET ACTIVE STATUS === \\
        [HttpPut("ActivateCategory/{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<SingleCategoryResponseDTO>> ActivateCategory(int id)
        {
            var activatedCategory = await _categoryService.ActivateCategory(id);
            return activatedCategory.StatusCode switch
            {
                200 => Ok(activatedCategory.Data),
                400 => BadRequest(activatedCategory.Message),
                404 => NotFound(activatedCategory.Message),
                500 => StatusCode(500, activatedCategory.Message),
                _ => StatusCode(activatedCategory.StatusCode, activatedCategory)
            };
        }

        [HttpPut("DeactivateCategory/{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<SingleCategoryResponseDTO>> DeactivateCategory(int id)
        {
            var deactivatedCategory = await _categoryService.DeactivateCategory(id);
            return deactivatedCategory.StatusCode switch
            {
                200 => Ok(deactivatedCategory.Data),
                400 => BadRequest(deactivatedCategory.Message),
                404 => NotFound(deactivatedCategory.Message),
                500 => StatusCode(500, deactivatedCategory.Message),
                _ => StatusCode(deactivatedCategory.StatusCode, deactivatedCategory)
            };
        }
    }
}
