using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.CategoryDTO_s;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.CategoryRepositories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        // === GET ===
        public async Task<ApiResponse<IEnumerable<BulkCategoryResponseDTO>>> GetAllCategories(CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieve all categories from the repository
                var categories = await _categoryRepository.GetAllCategoriesAsync(cancellationToken);

                // Check if categories were found and return the appropriate response
                return categories is { } && categories.Any()
                    ? Success(categories.Select(MapToBulkDto), "Categories retrieved successfully.", 200)
                    : Error<IEnumerable<BulkCategoryResponseDTO>>("No categories found.", 404);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return Error<IEnumerable<BulkCategoryResponseDTO>>("Internal error occurred, failed to load categories.", 500);
            }
        }

        public async Task<ApiResponse<SingleCategoryResponseDTO>> GetSingleCategory(int categoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                var findCategoryResult = await FindCategoryById(categoryId, cancellationToken);

                return findCategoryResult.Category is { } category
                    ? Success(MapToSingleDto(category), "Category retrieved successfully.", 200)
                    : findCategoryResult.Error!;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return Error<SingleCategoryResponseDTO>("Internal error occurred, failed to load category.", 500);
            }
        }

        // === POST ===
        public async Task<ApiResponse<SingleCategoryResponseDTO>> AddCategory(CreateCategoryRequestDTO dto, CancellationToken cancellationToken = default)
        {
            // Validate the input DTO
            var validationResponse = ValidateDtoExists(dto);
            if (validationResponse != null) return validationResponse;
            try
            {
                // Validate the category name is not null, empty, or whitespace and check for duplicates
                var nameValidationResponse = await ValidateNewCategoryName(dto.Name, cancellationToken);
                if (nameValidationResponse != null) return nameValidationResponse;

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
                var createdCategory = await _categoryRepository.CreateCategoryAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Return the created category details in the response DTO
                return Success(MapToSingleDto(createdCategory), "Category added successfully.", 201);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return Error<SingleCategoryResponseDTO>("Internal error occurred, failed to add category.", 500);
            }
        }

        // === PUT ===
        public async Task<ApiResponse<SingleCategoryResponseDTO>> UpdateCategoryDetails(int id, UpdateCategoryDetailsRequestDTO dto, CancellationToken cancellationToken = default)
        {
            // Validate the input DTO
            var dtoValidationResponse = ValidateDtoExists(dto);
            if (dtoValidationResponse != null) return dtoValidationResponse;

            // Validate the RowVersion for concurrency control
            var rowVersionValidationResponse = RowVersionHelper.ValidateFormat<SingleCategoryResponseDTO>(dto.RowVersion);
            if (rowVersionValidationResponse != null) return rowVersionValidationResponse;

            try
            {
                // Validate the updated category name is not null, empty, or whitespace and check for duplicates
                var nameValidationResponse = await ValidateUpdatedCategoryName(id, dto.Name, cancellationToken);
                if (nameValidationResponse != null) return nameValidationResponse;

                //Grab the category to update from the repository, and validate if it exists
                var fetchCategoryResult = await FindCategoryById(id, cancellationToken);
                if (fetchCategoryResult.Category == null) return fetchCategoryResult.Error!;
                //Assign the found category to a variable for easier access
                var category = fetchCategoryResult.Category;

                // Validate if the provided RowVersion matches the category's RowVersion for concurrency control
                var validateMatchingRowVersionResult = RowVersionHelper.Validate<SingleCategoryResponseDTO>(category.RowVersion, dto.RowVersion);
                if (validateMatchingRowVersionResult != null) return validateMatchingRowVersionResult;

                //Assign the updated values from the DTO to the category entity
                category.Name = dto.Name;
                category.Description = dto.Description;
                category.Updated = DateTime.UtcNow;

                // Save the updated category to the repository
                await _categoryRepository.UpdateCategoryAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                //Return the updated category details in the response DTO
                return Success(MapToSingleDto(category), "Category updated successfully.", 200);
            }
            catch(DbUpdateConcurrencyException)
            {
                return Error<SingleCategoryResponseDTO>("Concurrency conflict occurred. The category has been modified by another user.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return Error<SingleCategoryResponseDTO>("Internal error occurred, failed to update category.", 500);
            }
        }

        // === SET ACTIVE STATUS ===
        public async Task<ApiResponse<SingleCategoryResponseDTO>> ActivateCategory(int id, UpdateCategoryStatusRequestDTO dto, CancellationToken cancellationToken = default)
        {
            // Validate the input DTO RowVersion for concurrency control
            var validateRowVersion = RowVersionHelper.ValidateFormat<SingleCategoryResponseDTO>(dto.RowVersion);
            if (validateRowVersion != null) return validateRowVersion;

            try
            {
                //Validate if the category exists by using the helper method
                var validateCategoryResult = await FindCategoryById(id, cancellationToken);
                if (validateCategoryResult.Category == null) return validateCategoryResult.Error!;

                var category = validateCategoryResult.Category;

                //Validate if the provided RowVersion matches the category's RowVersion for concurrency control
                var validateMatchingRowVersionResult = RowVersionHelper.Validate<SingleCategoryResponseDTO>(category.RowVersion, dto.RowVersion);
                if (validateMatchingRowVersionResult != null) return validateMatchingRowVersionResult;

                //Validate if the category is already active
                if (category.IsActive || dto.IsActive)
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
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                //Return the activated category details in the response DTO
                return Success(MapToSingleDto(category), "Category activated successfully.", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Error<SingleCategoryResponseDTO>("Concurrency conflict occurred. The category has been modified by another user.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return Error<SingleCategoryResponseDTO>("Internal error occurred, failed to activate category.", 500);
            }
        }

        public async Task<ApiResponse<SingleCategoryResponseDTO>> DeactivateCategory(int id, UpdateCategoryStatusRequestDTO dto, CancellationToken cancellationToken = default)
        {
            // Validate the input DTO RowVersion for concurrency control
            var validateRowVersion = RowVersionHelper.ValidateFormat<SingleCategoryResponseDTO>(dto.RowVersion);
            if (validateRowVersion != null) return validateRowVersion;

            try
            {
                //Validate if the category exists by using the helper method
                var validateCategoryResult = await FindCategoryById(id, cancellationToken);
                if (validateCategoryResult.Category == null) return validateCategoryResult.Error!;

                var category = validateCategoryResult.Category;

                //Validate if the provided RowVersion matches the category's RowVersion for concurrency control
                var validateMatchingRowVersionResult = RowVersionHelper.Validate<SingleCategoryResponseDTO>(category.RowVersion, dto.RowVersion);
                if (validateMatchingRowVersionResult != null) return validateMatchingRowVersionResult;

                //Validate if the category is already inactive
                if (!category.IsActive || !dto.IsActive)
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
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                //Return the deactivated category details in the response DTO
                return Success(MapToSingleDto(category), "Category deactivated successfully.", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Error<SingleCategoryResponseDTO>("Concurrency conflict occurred. The category has been modified by another user.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return Error<SingleCategoryResponseDTO>("Internal error occurred, failed to deactivate category.", 500);
            }
        }

        
        
        // === API RESPONSE BUILDERS === \\
        private static ApiResponse<T> Success<T>(T data, string message, int statusCode = 200) =>
            ApiResponseHelper.Success(data, message, statusCode);

        private static ApiResponse<T> Error<T>(string message, int statusCode = 400) =>
            ApiResponseHelper.Failure<T>(message, statusCode);

        // === DTO MAPPERS === \\
        private static SingleCategoryResponseDTO MapToSingleDto(Category category) =>
            new SingleCategoryResponseDTO
            {
                ID = category.ID,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                RowVersion = category.RowVersion
            };

        private static BulkCategoryResponseDTO MapToBulkDto(Category category) =>
            new BulkCategoryResponseDTO
            {
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
            };

        // === Find Category By ID Helper Method ===

        /// <summary>
        /// Finds a category by its ID. and returns the category along with any error response if applicable.
        /// </summary>
        /// <param name="categoryId">The ID of the category to find.</param>
        /// <returns>
        /// Returns a tuple containing the found category (or null if not found) and an ApiResponse with error details (or null if no error).
        /// </returns>
        private async Task<(Category? Category, ApiResponse<SingleCategoryResponseDTO>? Error)> FindCategoryById(int categoryId, CancellationToken cancellationToken = default)
        {
            // Retrieve the category by ID from the repository
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId, cancellationToken);

            // Validate the categoryId
            if (category == null)
                return (null, Error<SingleCategoryResponseDTO>("Category not found.", 404));

            return (category, null);
        }

        // === VALIDATION METHODS ===
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
                return Error<SingleCategoryResponseDTO>("Request body is required.", 400);

            return null;
        }

        /// <summary>
        /// Validates if the provided category name is not null, empty, or whitespace, and checks if a category with the same name already exists in the repository.
        /// This method is used when adding a new category to ensure that the name is valid and unique.
        /// </summary>
        /// <param name="categoryName">The name of the category to validate.</param>
        /// <returns>An ApiResponse indicating a failure if the category name is invalid or already exists, otherwise null.</returns>
        private async Task<ApiResponse<SingleCategoryResponseDTO>?> ValidateNewCategoryName(string categoryName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return Error<SingleCategoryResponseDTO>("Category name is required.", 400);

            // Validate if a category with the same name already exists
            if (await _categoryRepository.CategoryNameExistsASync(categoryName, cancellationToken))
                return Error<SingleCategoryResponseDTO>("Category with the same name already exists.", 400);

            return null;
        }

        /// <summary>
        /// Validates if the updated category name is not null, empty, or whitespace, and checks if a category with the same name already exists in the repository (excluding the category being updated).
        /// This method is used when updating an existing category to ensure that the new name is valid and unique.
        /// </summary>
        /// <param name="id">The ID of the category being updated.</param>
        /// <param name="categoryName">The new name of the category to validate.</param>
        /// <returns>An ApiResponse indicating a failure if the new category name is invalid or already exists, otherwise null.</returns>
        private async Task<ApiResponse<SingleCategoryResponseDTO>?> ValidateUpdatedCategoryName(int id, string categoryName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return Error<SingleCategoryResponseDTO>("Category name is required.", 400);

            // Validate if a category with the same name already exists
            if (await _categoryRepository.OtherCategoryNameExistsAsync(id, categoryName, cancellationToken))
                return Error<SingleCategoryResponseDTO>("Category with the same name already exists.", 400);

            return null;
        }
    }
}
