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
                var productList = await _productRepo.GetAllProductsAsync();

                if (!productList.Any())
                {
                    return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                    {
                        Success = false,
                        Message = "No Products Found",
                        StatusCode = 404
                    };
                }

                List<BulkProductResponseDTO> dtoList = new List<BulkProductResponseDTO>();

                foreach (var product in productList)
                {
                    var productdto = new BulkProductResponseDTO
                    {
                        ID = product.ID,
                        Sku = product.Sku,
                        Name = product.Name,
                        QuantityInStock = product.QuantityInStock,
                        Price = product.Price,
                        IsActive = product.IsActive
                    };
                    dtoList.Add(productdto);
                }
                return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = true,
                    Message = "Successfully Retrieved All Products",
                    Data = dtoList,
                    StatusCode = 200
                };
            }
            catch(Exception)
            {
                return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<SingleProductResponseDTO>> GetSingleProduct(int productId)
        {
            if (!await _productRepo.ProductExistsAsync(productId))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Product Not Found",
                    StatusCode = 404
                };
            }
            try
            {
                var product = await _productRepo.GetProductAsync(productId);
                var productResponse = new SingleProductResponseDTO
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
                };
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = true,
                    Message = "Product Successfully Retrieved",
                    Data = productResponse,
                    StatusCode = 200
                };
            }
            catch (Exception)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkProductResponseDTO>>> GetProductsByCategory(int categoryId)
        {
            if (!await _categoryRepo.CategoryExistsAsync(categoryId))
            {
                return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = false,
                    Message = "Category Not Found",
                    StatusCode = 404
                };
            }
            try
            {
                var productList = await _productRepo.GetProductsByCategory(categoryId);
                if (!productList.Any())
                {
                    return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                    {
                        Success = false,
                        Message = "No Products Found for this Category",
                        StatusCode = 404
                    };
                }
                List<BulkProductResponseDTO> dtoList = new List<BulkProductResponseDTO>();
                foreach (var product in productList)
                {
                    var productdto = new BulkProductResponseDTO
                    {
                        ID = product.ID,
                        Sku = product.Sku,
                        Name = product.Name,
                        QuantityInStock = product.QuantityInStock,
                        Price = product.Price,
                        IsActive = product.IsActive
                    };
                    dtoList.Add(productdto);
                }
                return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = true,
                    Message = "Successfully Retrieved Products by Category",
                    Data = dtoList,
                    StatusCode = 200
                };
            }
            catch (Exception)
            {
                return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkProductResponseDTO>>> GetProductsBelowReorderLevel()
        {
            try
            {
                var reorderList = await _productRepo.GetProductsBelowReorderLevelAsync();

                if(!reorderList.Any())
                {
                    return(new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                    {
                        Success = false,
                        Message = "No Products Found Below Reorder Level",
                        StatusCode = 404
                    });
                }
                List<BulkProductResponseDTO> dtoList = new List<BulkProductResponseDTO>();

                foreach (var product in reorderList)
                {
                    var productdto = new BulkProductResponseDTO
                    {
                        ID = product.ID,
                        Sku = product.Sku,
                        Name = product.Name,
                        QuantityInStock = product.QuantityInStock,
                        Price = product.Price,
                        IsActive = product.IsActive
                    };
                    dtoList.Add(productdto);
                }
                return new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = true,
                    Message = "Successfully Retrieved Products Below Reorder Level",
                    Data = dtoList,
                    StatusCode = 200
                };
            }
            catch (Exception)
            {
                return(new ApiResponse<IEnumerable<BulkProductResponseDTO>>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                });
            }
        }

        // === POST === \\
        public async Task<ApiResponse<SingleProductResponseDTO>> AddProduct(CreateProductRequestDTO dto)
        {
            if(dto == null)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Request data was null",
                    StatusCode = 400
                };
            }
            if (string.IsNullOrWhiteSpace(dto.Sku) || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Description))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Please fill out all Product Fields",
                    StatusCode = 400
                };
            }
            if (dto.Price <= 0.00m)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Nothing is free, please Input Price value",
                    StatusCode = 400
                };
            }
            
            try
            {
                if (!await _supplierRepo.SupplierExistsAsync(dto.SupplierID))
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Supplier does not exist",
                        StatusCode = 404
                    };
                }
                if (!await _categoryRepo.CategoryExistsAsync(dto.CategoryID))
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Category does not exist",
                        StatusCode = 404
                    };
                }
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

                var createdProduct = await _productRepo.AddProductAsync(product);
                var response = new SingleProductResponseDTO
                {
                    ID = createdProduct.ID,
                    Sku = createdProduct.Sku,
                    Name = createdProduct.Name,
                    Description = createdProduct.Description,
                    CategoryID = createdProduct.CategoryID,
                    CategoryName = createdProduct.Category!.Name,
                    QuantityInStock = createdProduct.QuantityInStock,
                    ReorderLevel = createdProduct.ReorderLevel,
                    Price = createdProduct.Price,
                    SupplierID = createdProduct.SupplierID,
                    SupplierName = createdProduct.Supplier!.Name,
                    IsActive = createdProduct.IsActive

                };
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = true,
                    Message = "Product was successfully created",
                    Data = response,
                    StatusCode = 201

                };
            }
            catch(Exception)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success =false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            } 
        }

        // === PUT === \\
        public async Task<ApiResponse<SingleProductResponseDTO>> UpdateProductDetails(int id, UpdateProductDetailsRequestDTO dto)
        {
            if (! await _productRepo.ProductExistsAsync(id))
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Product Not Found",
                    StatusCode = 404
                };
            }
            try
            {
                var updateProduct = await _productRepo.GetProductAsync(id);
                updateProduct!.Sku = dto.Sku;
                updateProduct.Name = dto.Name;
                updateProduct.Description = dto.Description;
                updateProduct.CategoryID = dto.CategoryID;
                updateProduct.SupplierID = dto.SupplierID;

                var updateProductResult = await _productRepo.UpdateProductDetailsAsync(id, updateProduct);

                if (!updateProductResult)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Incorrect Data Input",
                        StatusCode = 400
                    };
                }

                var findUpdatedProduct = await _productRepo.GetProductAsync(id);

                var response = new SingleProductResponseDTO
                {
                    ID = findUpdatedProduct!.ID,
                    Sku = findUpdatedProduct.Sku,
                    Name = findUpdatedProduct.Name,
                    Description = findUpdatedProduct.Description,
                    CategoryID = findUpdatedProduct.CategoryID,
                    CategoryName = findUpdatedProduct.Category!.Name,
                    QuantityInStock = findUpdatedProduct.QuantityInStock,
                    ReorderLevel = findUpdatedProduct.ReorderLevel,
                    Price = findUpdatedProduct.Price,
                    SupplierID = findUpdatedProduct.SupplierID,
                    SupplierName = findUpdatedProduct.Supplier!.Name,
                    IsActive = findUpdatedProduct.IsActive
                };
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = true,
                    Message = "Successfully Update Prodcut Details",
                    Data = response,
                    StatusCode = 200
                };
            }
            catch (Exception)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }



        // === PATCH === \\
        public async Task<ApiResponse<SingleProductResponseDTO>> UpdateProductPrice(int id, UpdateProductPriceRequestDTO dto)
        {
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
                
                product.Price = dto.Price;
                await _productRepo.SaveChangesAsync();

                var response = new SingleProductResponseDTO
                {
                    ID = product.ID,
                    Sku = product.Sku,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryID = product.CategoryID,
                    CategoryName = product.Category!.Name,
                    QuantityInStock = product.QuantityInStock,
                    ReorderLevel = product.ReorderLevel,
                    Price = product.Price,
                    SupplierID = product.SupplierID,
                    SupplierName = product.Supplier!.Name,
                    IsActive = product.IsActive
                };

                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = true,
                    Message = "Price Successfully Updated",
                    Data = response,
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<SingleProductResponseDTO>> UpdateProductStockQuantity(int id, UpdateProductStockRequestDTO dto)
        {
            try
            {
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

                var response = new SingleProductResponseDTO
                {
                    ID = product.ID,
                    Sku = product.Sku,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryID = product.CategoryID,
                    CategoryName = product.Category!.Name,
                    QuantityInStock = product.QuantityInStock,
                    ReorderLevel = product.ReorderLevel,
                    Price = product.Price,
                    SupplierID = product.SupplierID,
                    SupplierName = product.Supplier!.Name,
                    IsActive = product.IsActive
                };

                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = true,
                    Message = "ReOrderStock Levels Updated",
                    Data = response,
                    StatusCode = 200
                };

            }
            catch (Exception)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<SingleProductResponseDTO>> UpdateProductReorderLevel(int id, UpdateProductReorderRequestDTO dto)
        {
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

                var response = new SingleProductResponseDTO
                {
                    ID = product.ID,
                    Sku = product.Sku,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryID = product.CategoryID,
                    CategoryName = product.Category!.Name,
                    QuantityInStock = product.QuantityInStock,
                    ReorderLevel = product.ReorderLevel,
                    Price = product.Price,
                    SupplierID = product.SupplierID,
                    SupplierName = product.Supplier!.Name,
                    IsActive = product.IsActive
                };

                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = true,
                    Message = "ReOrderStock Levels Updated",
                    Data = response,
                    StatusCode = 200
                };

            }
            catch (Exception)
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }

        // === SET ACTIVE STATUS === \\
        public async Task<ApiResponse<SingleProductResponseDTO>> ActivateProduct(int id)
        {
            try
            {
                var product = _productRepo.GetProductAsync(id).Result;
                if(product == null)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product Not Found",
                        StatusCode = 404
                    };
                }

                if (product.IsActive)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product is already active",
                        StatusCode = 400
                    };
                }

                product.IsActive = true;
                product.Updated = DateTime.UtcNow;
                await _productRepo.SaveChangesAsync();

                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = true,
                    Message = "Product activated successfully",
                    Data = new SingleProductResponseDTO
                    {
                        ID = product.ID,
                        Sku = product.Sku,
                        Name = product.Name,
                        Description = product.Description,
                        CategoryID = product.CategoryID,
                        CategoryName = product.Category!.Name,
                        QuantityInStock = product.QuantityInStock,
                        ReorderLevel = product.ReorderLevel,
                        Price = product.Price,
                        SupplierID = product.SupplierID,
                        SupplierName = product.Supplier!.Name,
                        IsActive = product.IsActive
                    },
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<SingleProductResponseDTO>> DeactivateProduct(int id)
        {
            try
            {
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

                if (!product.IsActive)
                {
                    return new ApiResponse<SingleProductResponseDTO>
                    {
                        Success = false,
                        Message = "Product is already inactive",
                        StatusCode = 400
                    };
                }

                product.IsActive = false;
                product.Updated = DateTime.UtcNow;
                await _productRepo.SaveChangesAsync();

                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = true,
                    Message = "Product deactivated successfully",
                    Data = new SingleProductResponseDTO
                    {
                        ID = product.ID,
                        Sku = product.Sku,
                        Name = product.Name,
                        Description = product.Description,
                        CategoryID = product.CategoryID,
                        CategoryName = product.Category!.Name,
                        QuantityInStock = product.QuantityInStock,
                        ReorderLevel = product.ReorderLevel,
                        Price = product.Price,
                        SupplierID = product.SupplierID,
                        SupplierName = product.Supplier!.Name,
                        IsActive = product.IsActive
                    },
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<SingleProductResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }
    }
}
