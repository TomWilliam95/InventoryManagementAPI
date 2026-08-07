using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PATCH;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PUT.STOCK;
using InventoryManagementAPI.Repositories.CategoryRepositories;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using InventoryManagementAPI.Repositories.SupplierRepositories;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.ProductRepositories
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly ISupplierRepository _supplierRepo;
        private readonly ICategoryRepository _categoryRepo;
        public ProductService(IProductRepository productRepo, ISupplierRepository supplierRepo, ICategoryRepository categoryRepo)
        {
            _productRepo = productRepo;
            _supplierRepo = supplierRepo;
            _categoryRepo = categoryRepo;
        }

        // === GET ===
        public async Task<ApiResponse<IEnumerable<BulkProductResponseDTO>>> GetAllProducts()
        {
            try
            {
                // Retrieve all products from the repository
                var productList = await _productRepo.GetAllProductsAsync();

                // Validate that the product list is not empty and return a response accordingly
                var productListResult = ValidateProductGroupExists(productList);
                if (productListResult.Products == null) return productListResult.Error!;

                // Return a successful response with the product list
                return BuildBulkProductResponse(productListResult.Products, "Successfully Retrieved All Products");
            }
            catch
            {
                return BuildCatchErrorResponseBulk("Internal error occurred, failed to load all products.");
            }
        }

        public async Task<ApiResponse<SingleProductResponseDTO>> GetSingleProduct(int productId)
        {
            try
            {
                // Retrieve the product from the repository using the provided productId
                var productResult = await ValidateProductExists(productId);
                if (productResult.Product == null) return productResult.Error!;

                // Return a successful response with the product details
                return BuildProductResponse(productResult.Product, "Product Successfully Retrieved", 200);
            }
            catch
            {
                return BuildCatchErrorResponseSingle("Internal error occurred, failed to load product.");
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkProductResponseDTO>>> GetProductsByCategory(int categoryId)
        {
            try
            {
                // Check if the category exists using the category repository
                if (!await _categoryRepo.CategoryExistsAsync(categoryId))
                {
                    return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                    {
                        Success = false,
                        Message = "Category Not Found",
                        StatusCode = 404
                    };
                }
                // Retrieve the list of products for the specified category from the product repository
                var productList = await _productRepo.GetProductsByCategoryAsync(categoryId);

                // Check if the product list is empty and return a response accordingly
                var productListResult = ValidateProductGroupExists(productList);
                if (productListResult.Products == null) return productListResult.Error!;

                // Return a successful response with the product list for the specified category
                return BuildBulkProductResponse(productListResult.Products, "Successfully Retrieved Products By Category");
            }
            catch
            {
                return BuildCatchErrorResponseBulk("Internal error occurred, failed to load products by category.");
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkProductResponseDTO>>> GetProductsBelowReorderLevel()
        {
            try
            {
                // Retrieve all products where the current stock is below the configured reorder level
                var reorderList = await _productRepo.GetProductsBelowReorderLevelAsync();

                // Check if any products need reordering and return a not found response if none exist
                var productListResult = ValidateProductGroupExists(reorderList);
                if (productListResult.Products == null) return productListResult.Error!;

                // Return a successful response with all products that are below their reorder level
                return BuildBulkProductResponse(productListResult.Products, "Successfully Retrieved Products Below Reorder Level");
            }
            catch
            {
                return BuildCatchErrorResponseBulk("Internal error occurred, failed to load products below reorder level.");
            }
        }

        // === POST ===
        public async Task<ApiResponse<SingleProductResponseDTO>> AddProduct(CreateProductRequestDTO dto)
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
                // Validate that the supplied SKU and Name dont exist
                // and the Category and Supplier exist before creating the product
                var validateExistenceResult = await ValidateDtoFieldsExist(dto);
                if (validateExistenceResult != null) return validateExistenceResult;

                // Create the product entity from the request DTO
                var product = new Product
                {
                    Sku = dto.Sku,
                    Name = dto.Name,
                    Description = dto.Description,
                    CategoryID = dto.CategoryID,
                    QuantityInStock = dto.QuantityInStock,
                    ReorderLevel = dto.ReorderLevel,
                    Price = dto.Price,
                    SupplierID = dto.SupplierID,
                    IsActive = dto.IsActive,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                };

                // Save the product, then reload it with category and supplier details for the response mapping
                var createdProduct = await _productRepo.AddProductAsync(product);
                await _productRepo.SaveChangesAsync();

                var createdProductWithDetails = await _productRepo.GetProductAsync(createdProduct.ID);
                if (createdProductWithDetails == null)
                {
                    return BuildCatchErrorResponseSingle("Product was created but could not be retrieved.");
                }

                // Return an Error ApiResponse if either the category or supplier details are missing after creation
                // Return a created response with the new product details
                return BuildProductResponse(createdProductWithDetails, "Product Successfully Created", 201);
            }
            catch
            {
                return BuildCatchErrorResponseSingle("Internal error occurred, failed to create product.");
            }
        }

        // === PUT ===
        public async Task<ApiResponse<SingleProductResponseDTO>> UpdateProductDetails(int id, UpdateProductDetailsRequestDTO dto)
        {
            // Validates DTO not null
            var validateDtoResult = ValidateDTO(dto);
            if (validateDtoResult != null) return validateDtoResult;

            // Validate that the RowVersion is provided for concurrency control
            var validateRowVersion = ValidateRowVersion(dto.RowVersion);
            if (validateRowVersion != null) return validateRowVersion;

            //validate the required fields before proceeding with the update
            var result = ValidateDtoFields(dto.Sku, dto.Name, dto.Description);
            if (result != null) return result;

            try
            {
                // Validate that the supplied supplier exists before updating the product
                var updateProductExistsResult = await ValidateProductExists(id);
                if (updateProductExistsResult.Product == null) return updateProductExistsResult.Error!;

                //Validates that the RowVersion provided in the DTO matches the RowVersion of the product in the database for concurrency control
                var validateRowVersionMatch = ValidateMatchRowVersion(updateProductExistsResult.Product, dto.RowVersion);
                if (validateRowVersionMatch != null) return validateRowVersionMatch;

                var validateExistenceResult = await UpdateValidateDtoFieldsExist(updateProductExistsResult.Product, dto);
                if (validateExistenceResult != null) return validateExistenceResult;
                var updateProduct = updateProductExistsResult.Product;


                // Update the product details with the values from the DTO
                updateProduct.Sku = dto.Sku;
                updateProduct.Name = dto.Name;
                updateProduct.Description = dto.Description;
                updateProduct.CategoryID = dto.CategoryID;
                updateProduct.SupplierID = dto.SupplierID;
                updateProduct.Updated = DateTime.UtcNow;

                // Persist the updated product details through the repository
                await _productRepo.SaveChangesAsync();

                // Reload the updated product with category and supplier details for the response
                var findUpdatedProduct = await _productRepo.GetProductAsync(updateProduct.ID);

                // Check if the updated product could be retrieved successfully
                if (findUpdatedProduct == null)
                {
                    return BuildCatchErrorResponseSingle("Product was updated but could not be retrieved.");
                }

                // Return an Error ApiResponse if either the category or supplier details are missing after the update
                // Return a successful response with the updated product details
                return BuildProductResponse(findUpdatedProduct, "Successfully Updated Product Details", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BuildConcurrencyCatchErrorResponse();
            }
            catch
            {
                return BuildCatchErrorResponseSingle("Internal error occurred, failed to update product details.");
            }
        }



        // === PATCH ===
        public async Task<ApiResponse<SingleProductResponseDTO>> UpdateProductPrice(int id, UpdateProductPriceRequestDTO dto)
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
            var validateRowVersion = ValidateRowVersion(dto.RowVersion);
            if (validateRowVersion != null) return validateRowVersion;

            try
            {
                // Load the product before applying the price update
                var validateProductResult = await ValidateProductExists(id);
                if (validateProductResult.Product == null) return validateProductResult.Error!;

                //Validates that the RowVersion provided in the DTO matches the RowVersion of the product in the database for concurrency control
                var validateRowVersionMatch = ValidateMatchRowVersion(validateProductResult.Product, dto.RowVersion);
                if (validateRowVersionMatch != null) return validateRowVersionMatch;

                // Update the price and save the change
                validateProductResult.Product.Price = dto.Price;
                validateProductResult.Product.Updated = DateTime.UtcNow;
                await _productRepo.SaveChangesAsync();

                // Return a successful response with the updated product details
                return BuildProductResponse(validateProductResult.Product, "Price Successfully Updated", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BuildConcurrencyCatchErrorResponse();
            }
            catch
            {
                return BuildCatchErrorResponseSingle("Internal error occurred, failed to update product price.");
            }
        }

        public async Task<ApiResponse<SingleProductResponseDTO>> UpdateProductReorderLevel(int id, UpdateProductReorderRequestDTO dto)
        {
            // Validate that the reorder level is not negative
            if (dto.ReorderLevel < 0)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Invalid Reorder Level",
                    StatusCode = 400
                };
            }

            // Validate that the RowVersion is provided for concurrency control
            var validateRowVersion = ValidateRowVersion(dto.RowVersion);
            if (validateRowVersion != null) return validateRowVersion;

            try
            {
                // Load the product before applying the reorder level update
                var productExistsResult = await ValidateProductExists(id);
                if (productExistsResult.Product == null) return productExistsResult.Error!;

                //Validates that the RowVersion provided in the DTO matches the RowVersion of the product in the database for concurrency control
                var validateRowVersionMatch = ValidateMatchRowVersion(productExistsResult.Product, dto.RowVersion);
                if (validateRowVersionMatch != null) return validateRowVersionMatch;

                // Update the ReorderLevel property of the product
                // Save the changes to the database
                productExistsResult.Product.ReorderLevel = dto.ReorderLevel;
                productExistsResult.Product.Updated = DateTime.UtcNow;
                await _productRepo.SaveChangesAsync();

                // Return a successful response with the updated reorder level details
                return BuildProductResponse(productExistsResult.Product, "ReOrderStock Levels Updated", 200);

            }
            catch (DbUpdateConcurrencyException)
            {
                return BuildConcurrencyCatchErrorResponse();
            }
            catch
            {
                return BuildCatchErrorResponseSingle("Internal error occurred, failed to update product reorder level.");
            }
        }

        // === SET ACTIVE STATUS ===
        public async Task<ApiResponse<SingleProductResponseDTO>> ActivateProduct(int id, UpdateProductStatusRequestDTO dto)
        {
            // Validate that the RowVersion is provided for concurrency control
            var validateRowVersion = ValidateRowVersion(dto.RowVersion);
            if (validateRowVersion != null) return validateRowVersion;

            try
            {
                // Load the product before attempting to activate it
                var productExistsResult = await ValidateProductExists(id);
                if (productExistsResult.Product == null) return productExistsResult.Error!;
                var product = productExistsResult.Product;

                //Validates that the RowVersion provided in the DTO matches the RowVersion of the product in the database for concurrency control
                var validateRowVersionMatch = ValidateMatchRowVersion(product, dto.RowVersion);
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
                await _productRepo.SaveChangesAsync();

                // Return a successful response with the activated product details
                return BuildProductResponse(product, "Product activated successfully", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BuildConcurrencyCatchErrorResponse();
            }
            catch
            {
                return BuildCatchErrorResponseSingle("Internal error occurred, failed to activate product.");
            }
        }

        public async Task<ApiResponse<SingleProductResponseDTO>> DeactivateProduct(int id, UpdateProductStatusRequestDTO dto)
        {
            // Validate that the RowVersion is provided for concurrency control
            var validateRowVersion = ValidateRowVersion(dto.RowVersion);
            if (validateRowVersion != null) return validateRowVersion;

            try
            {
                // Load the product before attempting to deactivate it
                var productExistsResult = await ValidateProductExists(id);
                if (productExistsResult.Product == null) return productExistsResult.Error!;
                var product = productExistsResult.Product;

                // Validates that the RowVersion provided in the DTO matches the RowVersion of the product in the database for concurrency control
                var validateRowVersionMatch = ValidateMatchRowVersion(product, dto.RowVersion);
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
                await _productRepo.SaveChangesAsync();

                // Return a successful response with the deactivated product details
                return BuildProductResponse(product, "Product deactivated successfully", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BuildConcurrencyCatchErrorResponse();
            }
            catch
            {
                return BuildCatchErrorResponseSingle("Internal error occurred, failed to deactivate product.");
            }
        }



        // === VALIDATION HELPER METHODS ===

        /// <summary>
        /// Validates whether a product with the specified ID exists in the repository.
        /// </summary>
        /// <param name="id">The ID of the product to validate.</param>
        /// <returns>A tuple containing the product (if found) and an ApiResponse (if not found).</returns>
        private async Task<(Product? Product, ApiResponse<SingleProductResponseDTO>? Error)> ValidateProductExists(int id)
        {
            var product = await _productRepo.GetProductAsync(id);

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
        private (IEnumerable<BulkProductResponseDTO>? Products, ApiResponse<IEnumerable<BulkProductResponseDTO>>? Error) ValidateProductGroupExists(IEnumerable<Product> products)
        {
            if (products == null || !products.Any())
            {
                return (null, new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = false,
                    Message = "No Products Found",
                    StatusCode = 404
                });
            }
            return (products.Select(p => new BulkProductResponseDTO
            {
                ID = p.ID,
                Sku = p.Sku,
                Name = p.Name,
                QuantityInStock = p.QuantityInStock,
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
        /// Validates the existence of related entities (SKU, Name, Supplier, Category) for a product DTO before creation.
        /// </summary>
        /// <param name="dto">The product DTO to validate.</param>
        /// <returns>Null if successful, otherwise an ApiResponse indicating the result of the validation failure.</returns>
        private async Task<ApiResponse<SingleProductResponseDTO>?> ValidateDtoFieldsExist(CreateProductRequestDTO dto)
        {
            // Validate that the supplied SKU is not already in use by another product
            if (await _productRepo.ProductSkuExistsAsync(dto.Sku))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Product with the same SKU already exists.",
                    StatusCode = 400
                };
            }
            // Validate that the supplied Name is not already in use by another product
            if (await _productRepo.ProductNameExistsAsync(dto.Name))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Product Name Already Exists",
                    StatusCode = 400
                };
            }

            // Validate that the supplied supplier exists before creating the product
            if (!await _supplierRepo.SupplierExistsAsync(dto.SupplierID))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Supplier does not exist",
                    StatusCode = 404
                };
            }
            // Validate that the supplied category exists before creating the product
            if (!await _categoryRepo.CategoryExistsAsync(dto.CategoryID))
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
        /// Validates the existence of related entities (SKU, Name, Supplier, Category) for a product DTO before updating an existing product.
        /// </summary>
        /// <param name="updateProduct">The existing product being updated.</param>
        /// <param name="dto">The product DTO containing the updated details.</param>
        /// <returns>Null if successful, otherwise an ApiResponse indicating the result of the validation failure.</returns>
        private async Task<ApiResponse<SingleProductResponseDTO>?> UpdateValidateDtoFieldsExist(Product updateProduct, UpdateProductDetailsRequestDTO dto)
        {
            // Validate that dto name is not already in use by another product, excluding the current product being updated
            if (await _productRepo.OtherProductNameExistsAsync(updateProduct.ID, dto.Name))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Product Name Already Exists",
                    StatusCode = 400
                };
            }
            // Validate that dto SKU is not already in use by another product, excluding the current product being updated
            if (await _productRepo.OtherProductSkuExistsAsync(updateProduct.ID, dto.Sku))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Product SKU Already Exists",
                    StatusCode = 400
                };
            }
            // Validate that the supplied category exists before updating the product
            if (!await _categoryRepo.CategoryExistsAsync(dto.CategoryID))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Category does not exist",
                    StatusCode = 404
                };
            }
            // Validate that the supplied supplier exists before updating the product
            if (!await _supplierRepo.SupplierExistsAsync(dto.SupplierID))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Supplier does not exist",
                    StatusCode = 404
                };
            }
            return null;
        }

        /// <summary>
        /// Validates that the provided RowVersion byte array is not null and has the expected length for concurrency control.
        /// </summary>
        /// <param name="rowVersion">The RowVersion byte array to validate.</param>
        /// <returns>An ApiResponse indicating the result of the validation.</returns>
        private ApiResponse<SingleProductResponseDTO>? ValidateRowVersion(byte[] rowVersion)
        {
            //Validate that the RowVersion is provided for concurrency control
            if (rowVersion == null || rowVersion.Length == 0)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "No RowVersion provided, unable to perform update",
                    StatusCode = 400
                };
            }
            //Validate that the RowVersion is exactly 8 bytes long, as expected for concurrency control
            if (rowVersion.Length != 8)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Invalid RowVersion provided, unable to perform update",
                    StatusCode = 400
                };
            }

            return null;
        }

        /// <summary>
        /// Validates that the provided RowVersion matches the RowVersion of the product in the database to ensure concurrency control.
        /// </summary>
        /// <param name="product">The product entity from the database.</param>
        /// <param name="rowVersion">The RowVersion byte array to validate.</param>
        /// <returns>An ApiResponse indicating the result of the validation.</returns>
        private ApiResponse<SingleProductResponseDTO>? ValidateMatchRowVersion(Product product, byte[] rowVersion)
        {
            if (!product.RowVersion.SequenceEqual(rowVersion))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "RowVersion mismatch, the product has been modified by another process.",
                    StatusCode = 409
                };
            }
            return null;
        }

        // === RESPONSE HELPER METHOD ===

        /// <summary>
        /// Creates an ApiResponse object for a single product, including its details and status information.
        /// Checks if the product's category and supplier are loaded before creating the response.
        /// Since product.Category and product.Supplier are virtual properties, they may not be loaded if lazy loading is not enabled or if they were not explicitly included in the query.
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
            if (product.Category is null || product.Supplier is null)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = $"{message}, failed to retrieve either category or supplier details",
                    StatusCode = 500
                };
            }

            return new ApiResponse<SingleProductResponseDTO>
            {
                Success = true,
                Message = message,
                Data = new SingleProductResponseDTO
                {
                    ID = product.ID,
                    Sku = product.Sku,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryID = product.CategoryID,
                    CategoryName = product.Category.Name,
                    QuantityInStock = product.QuantityInStock,
                    ReorderLevel = product.ReorderLevel,
                    Price = product.Price,
                    SupplierID = product.SupplierID,
                    SupplierName = product.Supplier.Name,
                    IsActive = product.IsActive,
                    RowVersion = product.RowVersion
                },
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Builds an error response for a single product operation, indicating that an internal error occurred.
        /// </summary>
        /// <param name="message">The error message to include in the response.</param>
        /// <returns>An ApiResponse indicating the internal error.</returns>
        private ApiResponse<SingleProductResponseDTO> BuildCatchErrorResponseSingle(string message)
        {
            return new ApiResponse<SingleProductResponseDTO>
            {
                Success = false,
                Message = message,
                StatusCode = 500
            };
        }
        /// <summary>
        /// Builds an error response for a single product operation, indicating that a concurrency error occurred during the update.
        /// </summary>
        /// <returns>An ApiResponse indicating the concurrency error.</returns>
        private ApiResponse<SingleProductResponseDTO> BuildConcurrencyCatchErrorResponse()
        {
            return new ApiResponse<SingleProductResponseDTO>
            {
                Success = false,
                Message = "Concurrency error occurred, failed to update product details.",
                StatusCode = 409
            };
        }

        /// <summary>
        /// Builds a successful response for bulk product operations, including a list of products and a success message.
        /// </summary>
        /// <param name="productDtoList">The list of product DTOs to include in the response.</param>
        /// <param name="message">The success message to include in the response.</param>
        /// <returns>An ApiResponse indicating the successful operation.</returns>
        private ApiResponse<IEnumerable<BulkProductResponseDTO>> BuildBulkProductResponse(IEnumerable<BulkProductResponseDTO> productDtoList, string message)
        {
            return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
            {
                Success = true,
                Message = message,
                Data = productDtoList,
                StatusCode = 200
            };
        }

        /// <summary>
        /// Builds an error response for bulk product operations, indicating that an internal error occurred.
        /// </summary>
        /// <param name="message">The error message to include in the response.</param>
        /// <returns>An ApiResponse indicating the internal error.</returns>
        private ApiResponse<IEnumerable<BulkProductResponseDTO>> BuildCatchErrorResponseBulk(string message)
        {
            return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
            {
                Success = false,
                Message = message,
                StatusCode = 500
            };
        } 
    }
}
