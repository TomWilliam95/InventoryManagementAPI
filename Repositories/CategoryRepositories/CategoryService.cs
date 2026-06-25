using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.CategoryDTO_s;

namespace InventoryManagementAPI.Repositories.CategoryRepositories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // === POST === \\
        public async Task<ApiResponse<SingleCategoryResponseDTO>> AddCategory(CreateCategoryRequestDTO dto)
        {
            if(dto == null)
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "Invalid category data.",
                    StatusCode = 400,
                };
            }
            if(string.IsNullOrWhiteSpace(dto.Name))
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "Category name is required.",
                    StatusCode = 400,
                };
            }
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                Created = DateOnly.FromDateTime(DateTime.UtcNow),
                Updated = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            try
            {
                var createdCategory = await _categoryRepository.CreateCategoryAsync(category);
                var responseDto = new SingleCategoryResponseDTO
                {
                    ID = createdCategory.ID,
                    Name = createdCategory.Name,
                    Description = createdCategory.Description,
                    IsActive = createdCategory.IsActive
                };
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = true,
                    Message = "Category added successfully.",
                    Data = responseDto,
                    StatusCode = 201,
                };
            }
            catch (Exception) { 
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred while adding the category.",
                    StatusCode = 500,
                };
            }
        }


        // === DELETE === \\
        public async Task<ApiResponse<object>> DeleteCategory(int id)
        {
            if (!await _categoryRepository.CategoryExistsAsync(id))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Category not found.",
                    StatusCode = 404,
                };
            }
            try
            {
                await _categoryRepository.DeleteCategoryAsync(id);
                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "Category deleted successfully.",
                    StatusCode = 204,
                };
            }
            catch(Exception)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the category.",
                    StatusCode = 500,
                };
            }
        }


        // === GET === \\
        public async Task<ApiResponse<IEnumerable<BulkCategoryResponseDTO>>> GetAllCategories()
        {
            try
            {
                var categories = await _categoryRepository.GetAllCategoriesAsync();
                if (categories == null)
                {
                    return new ApiResponse<IEnumerable<BulkCategoryResponseDTO>>
                    {
                        Success = false,
                        Message = "No categories found.",
                        StatusCode = 404,
                    };
                }
                List<BulkCategoryResponseDTO> responseDtoList = new List<BulkCategoryResponseDTO>();
                foreach (var category in categories)
                {
                    var categoryDto = new BulkCategoryResponseDTO
                    {
                        Name = category.Name,
                        Description = category.Description,
                        IsActive = category.IsActive
                    };
                    responseDtoList.Add(categoryDto);
                }
                return new ApiResponse<IEnumerable<BulkCategoryResponseDTO>>
                {
                    Success = true,
                    Message = "Categories retrieved successfully.",
                    Data = responseDtoList,
                    StatusCode = 200,
                };
            }
            catch (Exception)
            {
                return new ApiResponse<IEnumerable<BulkCategoryResponseDTO>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving categories.",
                    StatusCode = 500,
                };
            }
        }

        public async Task<ApiResponse<SingleCategoryResponseDTO>> GetSingleCategory(int categoryId)
        {
            if (!await _categoryRepository.CategoryExistsAsync(categoryId))
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "Category not found.",
                    StatusCode = 404,
                };
            }
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
                var responseDto = new SingleCategoryResponseDTO
                {
                    ID = category.ID,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive
                };
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = true,
                    Message = "Category retrieved successfully.",
                    Data = responseDto,
                    StatusCode = 200,
                };
            }
            catch (Exception)
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the category.",
                    StatusCode = 500,
                };
            }
        }

        // === PUT === \\
        public async Task<ApiResponse<SingleCategoryResponseDTO>> UpdateCategoryDetails(int id, UpdateCategoryDetailsRequestDTO dto)
        {
            if (!await _categoryRepository.CategoryExistsAsync(id))
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "Category not found.",
                    StatusCode = 404,
                };
            }
            try
            {
                var categoryToUpdate = await _categoryRepository.GetCategoryByIdAsync(id);
                categoryToUpdate.Name = dto.Name;
                categoryToUpdate.Description = dto.Description;
                categoryToUpdate.IsActive = dto.IsActive;
                categoryToUpdate.Updated = DateOnly.FromDateTime(DateTime.UtcNow);

                var updatedCategory = await _categoryRepository.UpdateCategoryAsync(categoryToUpdate.ID, categoryToUpdate);
                if(!updatedCategory)
                {
                    return new ApiResponse<SingleCategoryResponseDTO>
                    {
                        Success = false,
                        Message = "Failed to update the category, Invalid Input.",
                        StatusCode = 400,
                    };
                }
                var findUpdatedCategory = await _categoryRepository.GetCategoryByIdAsync(id);
                var responseDto = new SingleCategoryResponseDTO
                {
                    ID = findUpdatedCategory.ID,
                    Name = findUpdatedCategory.Name,
                    Description = findUpdatedCategory.Description,
                    IsActive = findUpdatedCategory.IsActive
                };
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = true,
                    Message = "Category updated successfully.",
                    Data = responseDto,
                    StatusCode = 200,
                };
            }
            catch (Exception)
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred while updating the category.",
                    StatusCode = 500,
                };
            }
        }
    }
}
