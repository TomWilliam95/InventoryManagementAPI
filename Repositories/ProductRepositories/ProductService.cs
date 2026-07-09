using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PUT.STOCK;
using InventoryManagementAPI.Repositories.CategoryRepositories;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using InventoryManagementAPI.Repositories.SupplierRepositories;

namespace InventoryManagementAPI.Repositories.ProductRepositories
{
    public class ProductService: IProductService
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

        // === GET === \\
        public async Task<ApiResponse<IEnumerable<BulkProductResponseDTO>>> GetAllProducts()
        {
            try
            {
                // Retrieve all products from the repository
                var productList = await _productRepo.GetAllProductsAsync();

                // Check if the product list is empty and return a response accordingly
                if (!productList.Any())
                {
                    return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                    {
                        Success = false,
                        Message = "No Products Found",
                        StatusCode = 404
                    };
                }
                // Map the product list to a list of BulkProductResponseDTO objects
                var productListResponse = productList.Select(p => new BulkProductResponseDTO
                {
                    ID = p.ID,
                    Sku = p.Sku,
                    Name = p.Name,
                    QuantityInStock = p.QuantityInStock,
                    Price = p.Price,
                    IsActive = p.IsActive
                }).ToList();

                // Return a successful response with the product list
                return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = true,
                    Message = "Successfully Retrieved All Products",
                    Data = productListResponse,
                    StatusCode = 200
                };
            }
            // Handle any exceptions that may occur while retrieving all products
            catch
            {
                return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to load products.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<SingleProductResponseDTO>> GetSingleProduct(int productId)
        {
            try
            {
                // Retrieve the product from the repository using the provided productId
                var product = await _productRepo.GetProductAsync(productId);
                // Check if the product is null (not found) and return a response accordingly
                if (product == null)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product Not Found",
                        StatusCode = 404
                    };
                }
                // Return a successful response with the product details
                return CreateProductResponse(product, "Product Successfully Retrieved", 200);
            }
            // Handle any exceptions that may occur while retrieving the product
            catch
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to load product.",
                    StatusCode = 500
                };
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
                var productList = await _productRepo.GetProductsByCategory(categoryId);
                // Check if the product list is empty and return a response accordingly
                if (!productList.Any())
                {
                    return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                    {
                        Success = false,
                        Message = "No Products Found for this Category",
                        StatusCode = 404
                    };
                }
                // Map the product list to a list of BulkProductResponseDTO objects
                var dtoList = productList.Select(p => new BulkProductResponseDTO
                {
                    ID = p.ID,
                    Sku = p.Sku,
                    Name = p.Name,
                    QuantityInStock = p.QuantityInStock,
                    Price = p.Price,
                    IsActive = p.IsActive
                }).ToList();

                // Return a successful response with the product list for the specified category
                return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = true,
                    Message = "Successfully Retrieved Products by Category",
                    Data = dtoList,
                    StatusCode = 200
                };
            }
            // Handle any exceptions that may occur while retrieving products by category
            catch
            {
                return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to load products by category.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkProductResponseDTO>>> GetProductsBelowReorderLevel()
        {
            try
            {
                // Retrieve all products where the current stock is below the configured reorder level
                var reorderList = await _productRepo.GetProductsBelowReorderLevelAsync();

                // Check if any products need reordering and return a not found response if none exist
                if(!reorderList.Any())
                {
                    return(new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                    {
                        Success = false,
                        Message = "No Products Found Below Reorder Level",
                        StatusCode = 404
                    });
                }

                // Create a list of DTOs that only exposes the summary fields needed for bulk product responses
                var dtoList = reorderList.Select(p => new BulkProductResponseDTO
                {
                    ID = p.ID,
                    Sku = p.Sku,
                    Name = p.Name,
                    QuantityInStock = p.QuantityInStock,
                    Price = p.Price,
                    IsActive = p.IsActive
                }).ToList();

                // Return a successful response with all products that are below their reorder level
                return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = true,
                    Message = "Successfully Retrieved Products Below Reorder Level",
                    Data = dtoList,
                    StatusCode = 200
                };
            }
            // Handle any exceptions that may occur while retrieving products below reorder level
            catch
            {
                return(new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to load products below reorder level.",
                    StatusCode = 500
                });
            }
        }

        // === POST === \\
        public async Task<ApiResponse<SingleProductResponseDTO>> AddProduct(CreateProductRequestDTO dto)
        {
            //Validate the request body and required fields before proceeding with product creation
            var result = ValidateDTO(dto, dto.Sku, dto.Name, dto.Description);
            if(result != null)
            {
                return result;
            }

            try
            {
                // Validate that the supplied SKU is not already in use by another product
                if (await _productRepo.AddProductSkuExistsAsync(dto.Sku))
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product SKU Already Exists",
                        StatusCode = 400
                    };
                }
                // Validate that the supplied Name is not already in use by another product
                if (await _productRepo.AddProductNameExistsAsync(dto.Name))
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
                    Created = DateOnly.FromDateTime(DateTime.Now),
                    Updated = DateTime.UtcNow
                };

                // Save the product, then reload it with category and supplier details for the response mapping
                var createdProduct = await _productRepo.AddProductAsync(product);
                var createdProductWithDetails = await _productRepo.GetProductAsync(createdProduct.ID);
                if (createdProductWithDetails == null)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product was created but could not be retrieved.",
                        StatusCode = 500
                    };
                }

                // Return a created response with the new product details
                return CreateProductResponse(createdProductWithDetails, "Product was successfully created", 201);
            }
            // Handle any exceptions that may occur while creating the product
            catch
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success =false,
                    Message = "Internal error occurred, failed to create product.",
                    StatusCode = 500
                };
            } 
        }

        // === PUT === \\
        public async Task<ApiResponse<SingleProductResponseDTO>> UpdateProductDetails(int id, UpdateProductDetailsRequestDTO dto)
        {
            //validate the request body and required fields before proceeding with the update
            var result = ValidateDTO(dto, dto.Sku, dto.Name, dto.Description);
            if(result != null)
            {
                return result;
            }

            try
            {
                // Validate that the supplied supplier exists before updating the product
                var updateProduct = await _productRepo.GetProductAsync(id);
                if(updateProduct == null)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product Not Found",
                        StatusCode = 404
                    };
                }
                // Validate that dto name is not already in use by another product, excluding the current product being updated
                if (await _productRepo.UpdateProductNameExistsAsync(updateProduct.ID, dto.Name))
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product Name Already Exists",
                        StatusCode = 400
                    };
                }
                // Validate that dto SKU is not already in use by another product, excluding the current product being updated
                if (await _productRepo.UpdateProductSkuExistsAsync(updateProduct.ID, dto.Sku))
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
                if(!await _supplierRepo.SupplierExistsAsync(dto.SupplierID))
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Supplier does not exist",
                        StatusCode = 404
                    };
                }

                // Update the product details with the values from the DTO
                updateProduct.Sku = dto.Sku;
                updateProduct.Name = dto.Name;
                updateProduct.Description = dto.Description;
                updateProduct.CategoryID = dto.CategoryID;
                updateProduct.SupplierID = dto.SupplierID;

                // Persist the updated product details through the repository
                await _productRepo.UpdateProductDetailsAsync(id, updateProduct);

                // Reload the updated product with category and supplier details for the response
                var findUpdatedProduct = await _productRepo.GetProductAsync(id);

                // Check if the updated product could be retrieved successfully
                if (findUpdatedProduct == null)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product was updated but could not be retrieved.",
                        StatusCode = 500
                    };
                }

                // Return a successful response with the updated product details
                return CreateProductResponse(findUpdatedProduct, "Successfully Update Prodcut Details", 200);
            }
            // Handle any exceptions that may occur while updating product details
            catch
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to update product details.",
                    StatusCode = 500
                };
            }
        }



        // === PATCH === \\
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
            try
            {
                // Load the product before applying the price update
                var product = await _productRepo.GetProductAsync(id);
                //validate that the product exists before attempting to update its price
                if (product == null)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product Not Found",
                        StatusCode = 404
                    };
                }
                
                // Update the price and save the change
                product.Price = dto.Price;
                await _productRepo.SaveChangesAsync();

                // Return a successful response with the updated product details
                return CreateProductResponse(product, "Price Successfully Updated", 200);
            }
            // Handle any exceptions that may occur while updating product price
            catch
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to update product price.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<SingleProductResponseDTO>> UpdateProductStockQuantity(int id, UpdateProductStockRequestDTO dto)
        {
            try
            {
                // Load the product before applying the stock quantity update
                var product = await _productRepo.GetProductAsync(id);

                if(product == null)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product Not Found",
                        StatusCode = 404
                    };
                }
                // Check if the requested stock value is different from the current stock value
                if(dto.QuantityInStock == product.QuantityInStock)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "No stock update!",
                        StatusCode = 400
                    };
                }

                // Update the QuantityInStock property of the product
                product.QuantityInStock = dto.QuantityInStock;
                await _productRepo.SaveChangesAsync();

                // Return a successful response with the updated product stock details
                return CreateProductResponse(product, "ReOrderStock Levels Updated", 200);

            }
            // Handle any exceptions that may occur while updating product stock quantity
            catch
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to update product stock quantity.",
                    StatusCode = 500
                };
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
            try
            {
                // Load the product before applying the reorder level update
                var product = await _productRepo.GetProductAsync(id);
                if (product == null)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product Not Found",
                        StatusCode = 404
                    };
                }
                // Update the ReorderLevel property of the product
                // Save the changes to the database
                product.ReorderLevel = dto.ReorderLevel;
                await _productRepo.SaveChangesAsync();

                // Return a successful response with the updated reorder level details
                return CreateProductResponse(product, "ReOrderStock Levels Updated", 200);

            }
            // Handle any exceptions that may occur while updating product reorder level
            catch
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to update product reorder level.",
                    StatusCode = 500
                };
            }
        }

        // === SET ACTIVE STATUS === \\
        public async Task<ApiResponse<SingleProductResponseDTO>> ActivateProduct(int id)
        {
            try
            {
                // Load the product before attempting to activate it
                var product = await _productRepo.GetProductAsync(id);
                if(product == null)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product Not Found",
                        StatusCode = 404
                    };
                }

                // Return a bad request response if the product is already active
                if (product.IsActive)
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
                return CreateProductResponse(product, "Product activated successfully", 200);
            }
            // Handle any exceptions that may occur while activating the product
            catch
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to activate product.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<SingleProductResponseDTO>> DeactivateProduct(int id)
        {
            try
            {
                // Load the product before attempting to deactivate it
                var product = await _productRepo.GetProductAsync(id);
                if(product == null)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product Not Found",
                        StatusCode = 404
                    };
                }

                // Return a bad request response if the product is already inactive
                if (!product.IsActive)
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
                return CreateProductResponse(product, "Product deactivated successfully", 200);
            }
            // Handle any exceptions that may occur while deactivating the product
            catch
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to deactivate product.",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Validates the provided product DTO and its required fields (SKU, Name, Description) for correctness and completeness.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="sku"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns>
        /// If validation fails, returns an ApiResponse object with a failure message and status code 400.
        /// If Successful, returns null indicating that the DTO is valid.
        /// </returns>
        private static ApiResponse<SingleProductResponseDTO>? ValidateDTO(Object dto, string sku, string name, string description)
        {
            // Validate that the request body was supplied
            if (dto == null)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Request data was null",
                    StatusCode = 400
                };
            }

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
            //Ive decided this system will use 8 character SKUs, so this validation is in place to ensure that all SKUs are exactly 8 characters long.
            if (sku.Length < 8)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "SKU must be 8 characters",
                    StatusCode = 400
                };
            }
            if(sku.Length > 8)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "SKU ,ust be 8 characters",
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

        // === RESPONSE HELPER METHOD === \\
        /// <summary>
        /// Creates an ApiResponse object for a single product, including its details and status information.
        /// Checks if the product's category and supplier are loaded before creating the response.
        /// Since product.Category and product.Supplier are virtual properties, they may not be loaded if lazy loading is not enabled or if they were not explicitly included in the query.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="message"></param>
        /// <param name="statusCode"></param>
        /// <returns>
        /// If unsuccesful, returns an ApiResponse object with a failure message and status code 500.
        /// If successful, returns an ApiResponse object with the product details, success message, and provided status code.
        /// </returns>
        private static ApiResponse<SingleProductResponseDTO> CreateProductResponse(Product product, string message, int statusCode)
        {
            if (product.Category is null || product.Supplier is null)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Product category or supplier was not loaded.",
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
                    IsActive = product.IsActive
                },
                StatusCode = statusCode
            };
        }
    }
}
