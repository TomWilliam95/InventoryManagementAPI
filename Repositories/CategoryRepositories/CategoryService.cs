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
            // Handle any exceptions that may occur while retrieving categories
            catch
            {
                return new ApiResponse<IEnumerable<BulkCategoryResponseDTO>>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to load categories.",
                    StatusCode = 500,
                };
            }
        }

        public async Task<ApiResponse<SingleCategoryResponseDTO>> GetSingleCategory(int categoryId)
        {
            try
            {
                // Use the helper method to find the category by ID and handle any errors
                var findCategoryResult = await FindCategoryById(categoryId);
                if (findCategoryResult.Category == null)
                {
                    // If the category is not found, return the error response from the helper method
                    return findCategoryResult.Error!;
                }

                // Return the category details in the response DTO
                return BuildDtoResponse(findCategoryResult.Category, "Category retrieved successfully.", 200);
            }
            catch
            {
                return BuildCatchErrorResponse("Internal error occurred, failed to load category.");
            }
        }

        // === POST === \\
        public async Task<ApiResponse<SingleCategoryResponseDTO>> AddCategory(CreateCategoryRequestDTO dto)
        {
            // Validate the input DTO
            var validationResponse = ValidateDtoExists(dto);
            if (validationResponse != null)
            {
                // If the DTO is null, return the validation error response
                return validationResponse;
            }
            try
            {
                // Validate the category name is not null, empty, or whitespace and check for duplicates
                var nameValidationResponse = await ValidateNewCategoryName(dto.Name);
                if (nameValidationResponse != null)
                {
                    // If the category name is invalid, return the validation error response
                    return nameValidationResponse;
                }

                // Create a new Category entity from the DTO
                var category = new Category
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    IsActive = dto.IsActive,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                };

                // Save the new category to the repository
                var createdCategory = await _categoryRepository.CreateCategoryAsync(category);

                // Return the created category details in the response DTO
                return BuildDtoResponse(createdCategory, "Category added successfully.", 201);
            }
            catch
            {
                return BuildCatchErrorResponse("Internal error occurred, failed to add category.");
            }
        }

        // === PUT === \\
        public async Task<ApiResponse<SingleCategoryResponseDTO>> UpdateCategoryDetails(int id, UpdateCategoryDetailsRequestDTO dto)
        {
            // Validate the input DTO
            var dtoValidationResponse = ValidateDtoExists(dto);
            if (dtoValidationResponse != null)
            {
                // If the DTO is null, return the validation error response
                return dtoValidationResponse;
            }
            if(dto.RowVersion == null || dto.RowVersion.Length == 0)
            {
                // If the RowVersion is null or empty, return a validation error response
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "RowVersion is required for concurrency control.",
                    StatusCode = 400,
                };
            }
            try
            {
                // Validate the updated category name is not null, empty, or whitespace and check for duplicates
                var nameValidationResponse = await ValidateUpdatedCategoryName(id, dto.Name);
                if(nameValidationResponse != null)
                {
                    // If the category name is invalid, return the validation error response
                    return nameValidationResponse;
                }

                //Grab the category to update from the repository, and validate if it exists
                var fetchCategoryResult = await FindCategoryById(id);
                if(fetchCategoryResult.Category == null)
                {
                    // If the category is not found, return the error response from the helper method
                    return fetchCategoryResult.Error!;
                }
                //Assign the found category to a variable for easier access
                var category = fetchCategoryResult.Category;

                if(!category.RowVersion.SequenceEqual(dto.RowVersion))
                {
                    // If the RowVersion does not match, return a concurrency error response
                    return new ApiResponse<SingleCategoryResponseDTO>
                    {
                        Success = false,
                        Message = "The category has been modified by another process. Please reload and try again.",
                        StatusCode = 409, // Conflict
                    };
                }

                //Assign the updated values from the DTO to the category entity
                category.Name = dto.Name;
                category.Description = dto.Description;
                category.Updated = DateTime.UtcNow;

                // Save the updated category to the repository
                await _categoryRepository.UpdateCategoryAsync(category);

                //Return the updated category details in the response DTO
                return BuildDtoResponse(category, "Category updated successfully.", 200);
            }
            catch
            {
                return BuildCatchErrorResponse("Internal error occurred, failed to update category.");
            }
        }

        // === SET ACTIVE STATUS === \\
        public async Task<ApiResponse<SingleCategoryResponseDTO>> ActivateCategory(int id)
        {
            try
            {
                //Validate if the category exists by using the helper method
                var validateCategoryResult = await FindCategoryById(id);
                if(validateCategoryResult.Category == null)
                {
                    return validateCategoryResult.Error!;
                }

                var category = validateCategoryResult.Category;

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

                //Set the category's IsActive property to true, update the Updated timestamp and save to database
                category.IsActive = true;
                category.Updated = DateTime.UtcNow;
                await _categoryRepository.SaveChangesAsync();

                //Return the activated category details in the response DTO
                return BuildDtoResponse(category, "Category activated successfully.", 200);
            }
            catch
            {
                return BuildCatchErrorResponse("Internal error occurred, failed to activate category.");
            }
        }

        public async Task<ApiResponse<SingleCategoryResponseDTO>> DeactivateCategory(int id)
        {
            try
            {
                //Validate if the category exists by using the helper method
                var validateCategoryResult = await FindCategoryById(id);
                if (validateCategoryResult.Category == null)
                {
                    return validateCategoryResult.Error!;
                }

                var category = validateCategoryResult.Category;

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
                category.Updated = DateTime.UtcNow;
                await _categoryRepository.SaveChangesAsync();

                //Return the deactivated category details in the response DTO
                return BuildDtoResponse(category, "Category deactivated successfully.", 200);
            }
            catch
            {
                return BuildCatchErrorResponse("Internal error occurred, failed to deactivate category.");
            }
        }



        // === Find Category By ID Helper Method === \\

        /// <summary>
        /// Finds a category by its ID. and returns the category along with any error response if applicable.
        /// </summary>
        /// <param name="categoryId">The ID of the category to find.</param>
        /// <returns>
        /// Returns a tuple containing the found category (or null if not found) and an ApiResponse with error details (or null if no error).
        /// </returns>
        private async Task<(Category? Category, ApiResponse<SingleCategoryResponseDTO>? Error)> FindCategoryById(int categoryId)
        {
            // Retrieve the category by ID from the repository
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);

            // Validate the categoryId
            if (category == null)
            {
                return (null, new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "Category not found.",
                    StatusCode = 404,
                });
            }
            return (category, null);
        }


        // === RESPONSE BUILDER METHODS === \\
        /// <summary>
        /// Builds an ApiResponse containing the details of a single category in the response DTO.
        /// </summary>
        /// <param name="category">The category entity to include in the response.</param>
        /// <param name="message">The message to include in the response.</param>
        /// <param name="statusCode">The HTTP status code to include in the response.</param>
        /// <returns>An ApiResponse containing the category details.</returns>
        private ApiResponse<SingleCategoryResponseDTO> BuildDtoResponse(Category category,string message, int statusCode)
        {
            // Return the category details in the response DTO
            return new ApiResponse<SingleCategoryResponseDTO>
            {
                Success = true,
                Message = message,
                Data = new SingleCategoryResponseDTO
                {
                    ID = category.ID,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive
                },
                StatusCode = statusCode,
            };
        }

        /// <summary>
        /// Builds an ApiResponse for error scenarios, indicating a failure with a provided message and a 500 status code.
        /// </summary>
        /// <param name="message">The error message to include in the response.</param>
        /// <returns>An ApiResponse indicating a failure.</returns>
        private ApiResponse<SingleCategoryResponseDTO> BuildCatchErrorResponse(string message)
        {
            return new ApiResponse<SingleCategoryResponseDTO>
            {
                Success = false,
                Message = message,
                StatusCode = 500,
            };
        }


        // === VALIDATION METHODS === \\
        /// <summary>
        /// Validates if the provided DTO exists (is not null). If it does not exist, 
        /// returns an ApiResponse indicating a failure with a 400 status code. 
        /// Otherwise, returns null.
        /// </summary>
        /// <param name="dto">The DTO to validate.</param>
        /// <returns>An ApiResponse indicating a failure if the DTO is null, otherwise null.</returns>
        private ApiResponse<SingleCategoryResponseDTO>? ValidateDtoExists(object? dto)
        {
            if (dto == null)
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "Request body is required.",
                    StatusCode = 400,
                };
            }
            return null;
        }

        /// <summary>
        /// Validates if the provided category name is not null, empty, or whitespace, and checks if a category with the same name already exists in the repository.
        /// This method is used when adding a new category to ensure that the name is valid and unique.
        /// </summary>
        /// <param name="categoryName">The name of the category to validate.</param>
        /// <returns>An ApiResponse indicating a failure if the category name is invalid or already exists, otherwise null.</returns>
        private async Task<ApiResponse<SingleCategoryResponseDTO>?> ValidateNewCategoryName(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "Category name is required.",
                    StatusCode = 400,
                };
            }
            // Validate if a category with the same name already exists
            if (await _categoryRepository.CategoryNameExistsASync(categoryName))
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "Category with the same name already exists.",
                    StatusCode = 400,
                };
            }
            return null;
        }

        /// <summary>
        /// Validates if the updated category name is not null, empty, or whitespace, and checks if a category with the same name already exists in the repository (excluding the category being updated).
        /// This method is used when updating an existing category to ensure that the new name is valid and unique.
        /// </summary>
        /// <param name="id">The ID of the category being updated.</param>
        /// <param name="categoryName">The new name of the category to validate.</param>
        /// <returns>An ApiResponse indicating a failure if the new category name is invalid or already exists, otherwise null.</returns>
        private async Task<ApiResponse<SingleCategoryResponseDTO>?> ValidateUpdatedCategoryName(int id, string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "Category name is required.",
                    StatusCode = 400,
                };
            }
            // Validate if a category with the same name already exists
            if (await _categoryRepository.OtherCategoryNameExistsAsync(id, categoryName))
            {
                return new ApiResponse<SingleCategoryResponseDTO>
                {
                    Success = false,
                    Message = "Category with the same name already exists.",
                    StatusCode = 400,
                };
            }
            return null;
        }
    }
}
