using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PATCH;
using InventoryManagementAPI.Repositories.CategoryRepositories;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using Microsoft.EntityFrameworkCore;
using InventoryManagementAPI.Services;
using InventoryManagementAPI.Models.Shared;
using InventoryManagementAPI.Models.Contracts.Products;

namespace InventoryManagementAPI.Repositories.ProductRepositories
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IUnitOfWork _unitOfWork;
        public ProductService(IProductRepository productRepo, ICategoryRepository categoryRepo, IUnitOfWork unitOfWork)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _unitOfWork = unitOfWork;
        }

        // === GET ===
        public async Task<ApiResponse<PagedResult<BulkProductResponseDTO>>> GetProducts(ProductQueryParameters query, CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieve all products from the repository
                var productList = await _productRepo.GetProductsAsync(query, cancellationToken);

                // Validate that the product list is not empty and return a response accordingly
                var productListResult = ValidateProductGroupExists(productList.Items);
                if (productListResult.Products == null) return productListResult.Error!;

                //Build the paged result with the product list and pagination details
                var page = new PagedResult<BulkProductResponseDTO>
                {
                    Items = productListResult.Products,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalItems = productList.TotalItems,
                };

                // Return a successful response with the product list
                return ApiResponseHelper.Success<PagedResult<BulkProductResponseDTO>>(page, "Successfully Retrieved All Products", 200);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<PagedResult<BulkProductResponseDTO>>("Internal error occurred, failed to load all products.", 500);
            }
        }

        public async Task<ApiResponse<SingleProductResponseDTO>> GetSingleProduct(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieve the product from the repository using the provided productId
                var productResult = await ValidateProductExists(productId, cancellationToken);
                if (productResult.Product == null) return productResult.Error!;

                // Return a successful response with the product details
                return BuildProductResponse(productResult.Product, "Product Successfully Retrieved", 200);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<SingleProductResponseDTO>("Internal error occurred, failed to load product.", 500);
            }
        }

        public async Task<ApiResponse<PagedResult<BulkProductResponseDTO>>> GetProductsByCategory(int categoryId, ProductQueryParameters query, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if the category exists using the category repository
                if (!await _categoryRepo.CategoryExistsAsync(categoryId, cancellationToken))
                    return ApiResponseHelper.Failure<PagedResult<BulkProductResponseDTO>>("Category does not exist", 404);

                // Retrieve the list of products for the specified category from the product repository
                var productList = await _productRepo.GetProductsByCategoryAsync(categoryId, query, cancellationToken);

                // Check if the product list is empty and return a response accordingly
                var productListResult = ValidateProductGroupExists(productList.Items);
                if (productListResult.Products == null) return productListResult.Error!;

                var page = new PagedResult<BulkProductResponseDTO>
                {
                    Items = productListResult.Products,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalItems = productList.TotalItems,
                };

                // Return a successful response with the product list for the specified category
                return ApiResponseHelper.Success<PagedResult<BulkProductResponseDTO>>(page, "Successfully Retrieved Products By Category", 200);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<PagedResult<BulkProductResponseDTO>>("Internal error occurred, failed to load products by category.", 500);
            }
        }

        public async Task<ApiResponse<PagedResult<BulkProductResponseDTO>>> GetProductsBelowReorderLevel(ProductQueryParameters query, CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieve all products where the current stock is below the configured reorder level
                var reorderList = await _productRepo.GetProductsBelowReorderLevelAsync(query, cancellationToken);

                // Check if any products need reordering and return a not found response if none exist
                var productListResult = ValidateProductGroupExists(reorderList.Items);
                if (productListResult.Products == null) return productListResult.Error!;

                // Return a successful response with all products that are below their reorder level
                return ApiResponseHelper.Success<PagedResult<BulkProductResponseDTO>>(new PagedResult<BulkProductResponseDTO>
                {
                    Items = productListResult.Products,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalItems = reorderList.TotalItems,
                }, 
                "Successfully Retrieved Products Below Reorder Level", 
                200);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<PagedResult<BulkProductResponseDTO>>("Internal error occurred, failed to load products below reorder level.", 500);
            }
        }

        // === POST ===
        public async Task<ApiResponse<SingleProductResponseDTO>> AddProduct(CreateProductRequestDTO dto, CancellationToken cancellationToken = default)
        {
            //Validates DTO not null
            var validateDtoResult = ValidateDTO(dto);
            if (validateDtoResult != null) return validateDtoResult;

            //Validate the required fields before proceeding with product creation
            var validateFieldsResult = ValidateDtoFields(dto.Sku, dto.Name, dto.Description);
            if (validateFieldsResult != null) return validateFieldsResult;

            //Validate Price
            if (dto.Price <= 0)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Price must be greater than zero",
                    StatusCode = 400
                };
            }

            try
            {
                // Validate that the supplied SKU and name are unique and the category exists.
                var validateExistenceResult = await ValidateDtoFieldsExist(dto, cancellationToken);
                if (validateExistenceResult != null) return validateExistenceResult;

                // Create the product entity from the request DTO
                var product = new Product
                {
                    Sku = dto.Sku,
                    Name = dto.Name,
                    Description = dto.Description,
                    CategoryID = dto.CategoryID,
                    Price = dto.Price,
                    IsActive = dto.IsActive,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                };

                // Save the product, then reload it with category details for response mapping.
                var createdProduct = await _productRepo.AddProductAsync(product, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var createdProductWithDetails = await _productRepo.GetProductAsync(createdProduct.ID, cancellationToken);
                if (createdProductWithDetails == null)
                {
                    return ApiResponseHelper.Failure<SingleProductResponseDTO>("Product was created but could not be retrieved.", 500);
                }

                // Return a created response with the new product details
                return BuildProductResponse(createdProductWithDetails, "Product Successfully Created", 201);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<SingleProductResponseDTO>("Internal error occurred, failed to create product.", 500);
            }
        }

        // === PUT ===
        public async Task<ApiResponse<SingleProductResponseDTO>> UpdateProductDetails(int id, UpdateProductDetailsRequestDTO dto, CancellationToken cancellationToken = default)
        {
            // Validates DTO not null
            var validateDtoResult = ValidateDTO(dto);
            if (validateDtoResult != null) return validateDtoResult;

            // Validate that the RowVersion is provided for concurrency control
            var validateRowVersion = RowVersionHelper.ValidateFormat<SingleProductResponseDTO>(dto.RowVersion);
            if (validateRowVersion != null) return validateRowVersion;

            //validate the required fields before proceeding with the update
            var result = ValidateDtoFields(dto.Sku, dto.Name, dto.Description);
            if (result != null) return result;

            try
            {
                // Validate that the product and its requested category exist.
                var updateProductExistsResult = await ValidateProductExists(id, cancellationToken);
                if (updateProductExistsResult.Product == null) return updateProductExistsResult.Error!;

                //Validates that the RowVersion provided in the DTO matches the RowVersion of the product in the database for concurrency control
                var validateRowVersionMatch = RowVersionHelper.Validate<SingleProductResponseDTO>(updateProductExistsResult.Product.RowVersion, dto.RowVersion);
                if (validateRowVersionMatch != null) return validateRowVersionMatch;

                var validateExistenceResult = await UpdateValidateDtoFieldsExist(updateProductExistsResult.Product, dto, cancellationToken);
                if (validateExistenceResult != null) return validateExistenceResult;
                var updateProduct = updateProductExistsResult.Product;


                // Update the product details with the values from the DTO
                updateProduct.Sku = dto.Sku;
                updateProduct.Name = dto.Name;
                updateProduct.Description = dto.Description;
                updateProduct.CategoryID = dto.CategoryID;
                updateProduct.Updated = DateTime.UtcNow;

                // Persist the updated product details through the repository
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Reload the updated product with category details for the response.
                var findUpdatedProduct = await _productRepo.GetProductAsync(updateProduct.ID, cancellationToken);

                // Check if the updated product could be retrieved successfully
                if (findUpdatedProduct == null)
                {
                    return ApiResponseHelper.Failure<SingleProductResponseDTO>("Product was updated but could not be retrieved.", 500);
                }

                // Return a successful response with the updated product details
                return BuildProductResponse(findUpdatedProduct, "Successfully Updated Product Details", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ApiResponseHelper.Failure<SingleProductResponseDTO>("Concurrency error occurred, failed to update product details.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<SingleProductResponseDTO>("Internal error occurred, failed to update product details.", 500);
            }
        }



        // === PATCH ===
        public async Task<ApiResponse<SingleProductResponseDTO>> UpdateProductPrice(int id, UpdateProductPriceRequestDTO dto, CancellationToken cancellationToken = default)
        {
            // Validate that the new product price is greater than zero
            if (dto.Price <= 0.00m)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Invalid Price Entry",
                    StatusCode = 400
                };
            }

            // Validate that the RowVersion is provided for concurrency control
            var validateRowVersion = RowVersionHelper.ValidateFormat<SingleProductResponseDTO>(dto.RowVersion);
            if (validateRowVersion != null) return validateRowVersion;

            try
            {
                // Load the product before applying the price update
                var validateProductResult = await ValidateProductExists(id, cancellationToken);
                if (validateProductResult.Product == null) return validateProductResult.Error!;

                //Validates that the RowVersion provided in the DTO matches the RowVersion of the product in the database for concurrency control
                var validateRowVersionMatch = RowVersionHelper.Validate<SingleProductResponseDTO>(validateProductResult.Product.RowVersion, dto.RowVersion);
                if (validateRowVersionMatch != null) return validateRowVersionMatch;

                // Update the price and save the change
                validateProductResult.Product.Price = dto.Price;
                validateProductResult.Product.Updated = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Return a successful response with the updated product details
                return BuildProductResponse(validateProductResult.Product, "Price Successfully Updated", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ApiResponseHelper.Failure<SingleProductResponseDTO>("Concurrency error occurred, failed to update product details.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<SingleProductResponseDTO>("Internal error occurred, failed to update product price.", 500);
            }
        }

        // === SET ACTIVE STATUS ===
        public async Task<ApiResponse<SingleProductResponseDTO>> ActivateProduct(int id, UpdateProductStatusRequestDTO dto, CancellationToken cancellationToken = default)
        {
            // Validate that the RowVersion is provided for concurrency control
            var validateRowVersion = RowVersionHelper.ValidateFormat<SingleProductResponseDTO>(dto.RowVersion);
            if (validateRowVersion != null) return validateRowVersion;

            try
            {
                // Load the product before attempting to activate it
                var productExistsResult = await ValidateProductExists(id, cancellationToken);
                if (productExistsResult.Product == null) return productExistsResult.Error!;
                var product = productExistsResult.Product;

                //Validates that the RowVersion provided in the DTO matches the RowVersion of the product in the database for concurrency control
                var validateRowVersionMatch = RowVersionHelper.Validate<SingleProductResponseDTO>(product.RowVersion, dto.RowVersion);
                if (validateRowVersionMatch != null) return validateRowVersionMatch;

                // Return a bad request response if the product is already active
                if (product.IsActive || dto.IsActive)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product is already active",
                        StatusCode = 400
                    };
                }

                // Set the product active and update the timestamp
                product.IsActive = true;
                product.Updated = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Return a successful response with the activated product details
                return BuildProductResponse(product, "Product activated successfully", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ApiResponseHelper.Failure<SingleProductResponseDTO>("Concurrency error occurred, failed to update product details.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<SingleProductResponseDTO>("Internal error occurred, failed to activate product.", 500);
            }
        }

        public async Task<ApiResponse<SingleProductResponseDTO>> DeactivateProduct(int id, UpdateProductStatusRequestDTO dto, CancellationToken cancellationToken = default)
        {
            // Validate that the RowVersion is provided for concurrency control
            var validateRowVersion = RowVersionHelper.ValidateFormat<SingleProductResponseDTO>(dto.RowVersion);
            if (validateRowVersion != null) return validateRowVersion;

            try
            {
                // Load the product before attempting to deactivate it
                var productExistsResult = await ValidateProductExists(id, cancellationToken);
                if (productExistsResult.Product == null) return productExistsResult.Error!;
                var product = productExistsResult.Product;

                // Validates that the RowVersion provided in the DTO matches the RowVersion of the product in the database for concurrency control
                var validateRowVersionMatch = RowVersionHelper.Validate<SingleProductResponseDTO>(product.RowVersion, dto.RowVersion);
                if (validateRowVersionMatch != null) return validateRowVersionMatch;

                // Return a bad request response if the product is already inactive
                if (!product.IsActive || !dto.IsActive)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product is already inactive",
                        StatusCode = 400
                    };
                }

                // Set the product inactive and update the timestamp
                product.IsActive = false;
                product.Updated = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Return a successful response with the deactivated product details
                return BuildProductResponse(product, "Product deactivated successfully", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ApiResponseHelper.Failure<SingleProductResponseDTO>("Concurrency error occurred, failed to update product details.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<SingleProductResponseDTO>("Internal error occurred, failed to deactivate product.", 500);
            }
        }



        // === VALIDATION HELPER METHODS ===

        /// <summary>
        /// Validates whether a product with the specified ID exists in the repository.
        /// </summary>
        /// <param name="id">The ID of the product to validate.</param>
        /// <returns>A tuple containing the product (if found) and an ApiResponse (if not found).</returns>
        private async Task<(Product? Product, ApiResponse<SingleProductResponseDTO>? Error)> ValidateProductExists(int id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepo.GetProductAsync(id, cancellationToken);

            if (product == null)
            {
                return (null, new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Product Not Found",
                    StatusCode = 404
                });
            }
            return (product, null);
        }

        /// <summary>
        /// Validates the existence of product groups and retrieves all products if available.
        /// </summary>
        /// <param name="products">The collection of products to validate.</param>
        /// <returns>A tuple containing a collection of BulkProductResponseDTO objects and null if products exist;
        /// otherwise, null and an ApiResponse indicating failure.</returns>
        private (IEnumerable<BulkProductResponseDTO>? Products, ApiResponse<PagedResult<BulkProductResponseDTO>>? Error) ValidateProductGroupExists(IEnumerable<Product> products)
        {
            if (products == null || !products.Any())
                return (null, ApiResponseHelper.Success<PagedResult<BulkProductResponseDTO>>(new PagedResult<BulkProductResponseDTO> { 
                    Items = new List<BulkProductResponseDTO>(),
                    Page = 1,
                    PageSize = 0,
                    TotalItems = 0,
                }, "No Products Found, Returned Empty Product List", 200));

            return (products.Select(p => new BulkProductResponseDTO
            {
                ID = p.ID,
                Sku = p.Sku,
                Name = p.Name,
                Price = p.Price,
                IsActive = p.IsActive
            }).ToList(), null);
        }
        /// <summary>
        /// Validates the provided DTO object is not null.
        /// </summary>
        /// <param name="dto">The DTO object to validate.</param>
        /// <returns>An ApiResponse object if the DTO is invalid, otherwise null.</returns>
        private ApiResponse<SingleProductResponseDTO>? ValidateDTO(object? dto)
        {
            if (dto == null)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Invalid DTO",
                    StatusCode = 400
                };
            }
            return null;
        }

        /// <summary>
        /// Validates the provided product DTO fields (SKU, Name, Description) for correctness and completeness.
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns>
        /// If validation fails, returns an ApiResponse object with a failure message and status code 400.
        /// If Successful, returns null indicating that the DTO is valid.
        /// </returns>
        private static ApiResponse<SingleProductResponseDTO>? ValidateDtoFields(string sku, string name, string description)
        {
            // Validate required product text fields before checking related entities
            if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Please fill out all Product Fields",
                    StatusCode = 400
                };
            }
            // Validate the length of the SKU field
            // I've decided this system will use 8 character SKUs, so this validation is in place to ensure that all SKUs are exactly 8 characters long.
            if (sku.Length < 8)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "SKU must be 8 characters",
                    StatusCode = 400
                };
            }
            if (sku.Length > 8)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "SKU must be 8 characters",
                    StatusCode = 400
                };
            }
            // Validate the length of the product name
            if (name.Length < 3)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Name must be at least 3 characters",
                    StatusCode = 400
                };
            }
            if (name.Length > 100)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Name cannot exceed 100 characters",
                    StatusCode = 400
                };
            }

            // Validate the length of the product description
            if (description.Length < 10)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Description must be at least 10 characters",
                    StatusCode = 400
                };
            }
            if (description.Length > 500)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Description cannot exceed 500 characters",
                    StatusCode = 400
                };
            }
            return null;
        }

        /// <summary>
        /// Validates product uniqueness and category existence before creation.
        /// </summary>
        /// <param name="dto">The product DTO to validate.</param>
        /// <returns>Null if successful, otherwise an ApiResponse indicating the result of the validation failure.</returns>
        private async Task<ApiResponse<SingleProductResponseDTO>?> ValidateDtoFieldsExist(CreateProductRequestDTO dto, CancellationToken cancellationToken = default)
        {
            // Validate that the supplied SKU is not already in use by another product
            if (await _productRepo.ProductSkuExistsAsync(dto.Sku, cancellationToken))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Product with the same SKU already exists.",
                    StatusCode = 400
                };
            }
            // Validate that the supplied Name is not already in use by another product
            if (await _productRepo.ProductNameExistsAsync(dto.Name, cancellationToken))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Product Name Already Exists",
                    StatusCode = 400
                };
            }

            // Validate that the supplied category exists before creating the product
            if (!await _categoryRepo.CategoryExistsAsync(dto.CategoryID, cancellationToken))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Category does not exist",
                    StatusCode = 404
                };
            }
            return null;
        }

        /// <summary>
        /// Validates product uniqueness and category existence before updating an existing product.
        /// </summary>
        /// <param name="updateProduct">The existing product being updated.</param>
        /// <param name="dto">The product DTO containing the updated details.</param>
        /// <returns>Null if successful, otherwise an ApiResponse indicating the result of the validation failure.</returns>
        private async Task<ApiResponse<SingleProductResponseDTO>?> UpdateValidateDtoFieldsExist(Product updateProduct, UpdateProductDetailsRequestDTO dto, CancellationToken cancellationToken = default)
        {
            // Validate that dto name is not already in use by another product, excluding the current product being updated
            if (await _productRepo.OtherProductNameExistsAsync(updateProduct.ID, dto.Name, cancellationToken))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Product Name Already Exists",
                    StatusCode = 400
                };
            }
            // Validate that dto SKU is not already in use by another product, excluding the current product being updated
            if (await _productRepo.OtherProductSkuExistsAsync(updateProduct.ID, dto.Sku, cancellationToken))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Product SKU Already Exists",
                    StatusCode = 400
                };
            }
            // Validate that the supplied category exists before updating the product
            if (!await _categoryRepo.CategoryExistsAsync(dto.CategoryID, cancellationToken))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Category does not exist",
                    StatusCode = 404
                };
            }
            return null;
        }

        // === RESPONSE HELPER METHOD ===

        /// <summary>
        /// Creates an ApiResponse object for a single product, including its details and status information.
        /// Checks that the product category is loaded before creating the response.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="message"></param>
        /// <param name="statusCode"></param>
        /// <returns>
        /// If unsuccessful, returns an ApiResponse object with a failure message and status code 500.
        /// If successful, returns an ApiResponse object with the product details, success message, and provided status code.
        /// </returns>
        ///
        private static ApiResponse<SingleProductResponseDTO> BuildProductResponse(Product product, string message, int statusCode)
        {
            if (product.Category is null)
            {
                return ApiResponseHelper.Failure<SingleProductResponseDTO>($"{message}, failed to retrieve category details", 500);
            }

            return ApiResponseHelper.Success(new SingleProductResponseDTO
                {
                    ID = product.ID,
                    Sku = product.Sku,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryID = product.CategoryID,
                    CategoryName = product.Category.Name,
                    Price = product.Price,
                    IsActive = product.IsActive,
                    RowVersion = product.RowVersion
                }, message, statusCode);
        }
    }
}
