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

        // === GET === \\
        public async Task<ApiResponse<IEnumerable<BulkCategoryResponseDTO>>> GetAllCategories()
        {
            try
            {
                // Retrieve all categories from the repository
                var categories = await _categoryRepository.GetAllCategoriesAsync();

                //validate if categories is null or empty
                if (categories == null || !categories.Any())
                {
                    return new ApiResponse<IEnumerable<BulkCategoryResponseDTO>>
                    {
                        Success = false,
                        Message = "No categories found.",
                        StatusCode = 404,
                    };
                }

                //Build the response DTO list from the retrieved categories
                var responseDtoList = categories.Select(categories => new BulkCategoryResponseDTO
                {
                    Name = categories.Name,
                    Description = categories.Description,
                    IsActive = categories.IsActive
                }).ToList();

                //Return the response DTO list in the ApiResponse
                return new ApiResponse<IEnumerable<BulkCategoryResponseDTO>>
                {
                    Success = true,
                    Message = "Categories retrieved successfully.",
                    Data = responseDtoList,
                    StatusCode = 200,
                };
            }
            // Handle any exceptions that may occur during the process and return an error response
            catch
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
            try
            {
                // Retrieve the category by ID from the repository
                var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);

                // Validate the categoryId
                if (category == null)
                {
                    return new ApiResponse<SingleCategoryResponseDTO>
                    {
                        Success = false,
                        Message = "Category not found.",
                        StatusCode = 404,
                    };
                }

                // Return the category details in the response DTO
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = true,
                    Message = "Category retrieved successfully.",
                    Data = new SingleCategoryResponseDTO
                    {
                        ID = category.ID,
                        Name = category.Name,
                        Description = category.Description,
                        IsActive = category.IsActive
                    },
                    StatusCode = 200,
                };
            }
            // Handle any exceptions that may occur during the process and return an error response
            catch
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the category.",
                    StatusCode = 500,
                };
            }
        }

        // === POST === \\
        public async Task<ApiResponse<SingleCategoryResponseDTO>> AddCategory(CreateCategoryRequestDTO dto)
        {
            // Validate the input DTO
            if (dto == null)
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "Invalid category data.",
                    StatusCode = 400,
                };
            }

            // Validate the category name
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "Category name is required.",
                    StatusCode = 400,
                };
            }
            try
            {
                // Validate if a category with the same name already exists
                if (await _categoryRepository.CategoryNameExistsAsync(dto.Name))
                {
                    return new ApiResponse<SingleCategoryResponseDTO>
                    {
                        Success = false,
                        Message = "Category with the same name already exists.",
                        StatusCode = 400,
                    };
                }

                // Create a new Category entity from the DTO
                var category = new Category
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    IsActive = dto.IsActive,
                    Created = DateOnly.FromDateTime(DateTime.UtcNow),
                    Updated = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                // Save the new category to the repository
                var createdCategory = await _categoryRepository.CreateCategoryAsync(category);

                // Return the created category details in the response DTO
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = true,
                    Message = "Category added successfully.",
                    Data = new SingleCategoryResponseDTO
                    {
                        ID = createdCategory.ID,
                        Name = createdCategory.Name,
                        Description = createdCategory.Description,
                        IsActive = createdCategory.IsActive
                    },
                    StatusCode = 201,
                };
            }
            // Handle any exceptions that may occur during the process and return an error response
            catch
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred while adding the category.",
                    StatusCode = 500,
                };
            }
        }

        // === PUT === \\
        public async Task<ApiResponse<SingleCategoryResponseDTO>> UpdateCategoryDetails(int id, UpdateCategoryDetailsRequestDTO dto)
        {
            // Validate the input DTO
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name == null)
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "Category name is required.",
                    StatusCode = 400,
                };
            }
            try
            {
                //Grab the category to update from the repository
                var category = await _categoryRepository.GetCategoryByIdAsync(id);

                // Validate if the category exists
                if (category == null)
                {
                    return new ApiResponse<SingleCategoryResponseDTO>
                    {
                        Success = false,
                        Message = "Category not found.",
                        StatusCode = 404,
                    };
                }

                // Validate if a category with the same name already exists 
                if (await _categoryRepository.CategoryNameExistsAsync(dto.Name) || string.Equals(category.Name,dto.Name))
                {
                    return new ApiResponse<SingleCategoryResponseDTO>
                    {
                        Success = false,
                        Message = "Category with the same name already exists.",
                        StatusCode = 400,
                    };
                }

                //Assign the updated values from the DTO to the category entity
                category.Name = dto.Name;
                category.Description = dto.Description;
                category.Updated = DateOnly.FromDateTime(DateTime.UtcNow);

                // Save the updated category to the repository
                await _categoryRepository.UpdateCategoryAsync(category.ID, category);

                //Return the updated category details in the response DTO
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = true,
                    Message = "Category updated successfully.",
                    Data = new SingleCategoryResponseDTO
                    {
                        ID = category.ID,
                        Name = category.Name,
                        Description = category.Description,
                        IsActive = category.IsActive
                    },
                    StatusCode = 200,
                };
            }
            // Handle any exceptions that may occur during the process and return an error response
            catch
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred while updating the category.",
                    StatusCode = 500,
                };
            }
        }

        // === SET ACTIVE STATUS === \\
        public async Task<ApiResponse<SingleCategoryResponseDTO>> ActivateCategory(int id)
        {
            try
            {
                //Retrieve the category by ID from the repository
                var category = await _categoryRepository.GetCategoryByIdAsync(id);

                //Validate if the category exists
                if (category == null)
                {
                    return new ApiResponse<SingleCategoryResponseDTO>
                    {
                        Success = false,
                        Message = "Category not found.",
                        StatusCode = 404,
                    };
                }

                //Validate if the category is already active
                if (category.IsActive)
                {
                    return new ApiResponse<SingleCategoryResponseDTO>
                    {
                        Success = false,
                        Message = "Category is already active.",
                        StatusCode = 400,
                    };
                }

                //Set the category's IsActive property to true and update the Updated timestamp
                category.IsActive = true;
                category.Updated = DateOnly.FromDateTime(DateTime.UtcNow);
                await _categoryRepository.SaveChangesAsync();

                //Return the activated category details in the response DTO
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = true,
                    Message = "Category activated successfully.",
                    Data = new SingleCategoryResponseDTO
                    {
                        ID = category.ID,
                        Name = category.Name,
                        Description = category.Description,
                        IsActive = category.IsActive
                    },
                    StatusCode = 200,
                };
            }
            // Handle any exceptions that may occur during the process and return an error response
            catch
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred while activating the category.",
                    StatusCode = 500,
                };
            }
        }

        public async Task<ApiResponse<SingleCategoryResponseDTO>> DeactivateCategory(int id)
        {
            try
            {
                //Retrieve the category by ID from the repository
                var category = await _categoryRepository.GetCategoryByIdAsync(id);

                //Validate if the category exists
                if (category == null)
                {
                    return new ApiResponse<SingleCategoryResponseDTO>
                    {
                        Success = false,
                        Message = "Category not found.",
                        StatusCode = 404,
                    };
                }

                //Validate if the category is already inactive
                if (!category.IsActive)
                {
                    return new ApiResponse<SingleCategoryResponseDTO>
                    {
                        Success = false,
                        Message = "Category is already inactive.",
                        StatusCode = 400,
                    };
                }

                //Set the category's IsActive property to false and update the Updated timestamp
                category.IsActive = false;
                category.Updated = DateOnly.FromDateTime(DateTime.UtcNow);
                await _categoryRepository.SaveChangesAsync();

                //Return the deactivated category details in the response DTO
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = true,
                    Message = "Category deactivated successfully.",
                    Data = new SingleCategoryResponseDTO
                    {
                        ID = category.ID,
                        Name = category.Name,
                        Description = category.Description,
                        IsActive = category.IsActive
                    },
                    StatusCode = 200,
                };
            }
            // Handle any exceptions that may occur during the process and return an error response
            catch
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred while deactivating the category.",
                    StatusCode = 500,
                };
            }
        }
    }
}
