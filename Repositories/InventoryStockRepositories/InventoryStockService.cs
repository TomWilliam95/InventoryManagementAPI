using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.InventoryStockDTO_s;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using InventoryManagementAPI.Repositories.WarehouseRepositories;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.InventoryStockRepositories
{
    public class InventoryStockService : IInventoryStockService
    {
        private readonly IInventoryStockRepository _inventoryStockRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IUnitOfWork _unitOfWork;
        public InventoryStockService(IInventoryStockRepository inventoryStockRepository, IProductRepository productRepository, IWarehouseRepository warehouseRepository, IUnitOfWork unitOfWork)
        {
            _inventoryStockRepository = inventoryStockRepository;
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<InventoryStockResponseDTO>> GetInventoryStockByProductAndWarehouseIdAsync(int productId, int warehouseId, CancellationToken cancellationToken = default)
        {
            try
            {
                //Validate Product
                var productResult = await GetProductAsync(productId, cancellationToken);
                if(productResult.Error != null) return productResult.Error;

                //Validate Warehouse
                var warehouseResult = await GetWarehouseAsync(warehouseId, cancellationToken);
                if(warehouseResult.Error != null) return warehouseResult.Error;

                // Validate Inventory Stock
                var inventoryStock = await _inventoryStockRepository.GetStockByProductAndWarehouseIDAsync(productId, warehouseId, cancellationToken);
                if(inventoryStock == null) return BuildErrorResponse($"Inventory stock for Product ID {productId} and Warehouse ID {warehouseId} not found.", 404);

                //Map to DTO and return success response
                var responseDTO = BuildInventoryStockResponseDTO(inventoryStock);
                return BuildSuccessResponse(responseDTO, "Inventory stock retrieved successfully.", 200);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch {
                return BuildErrorResponse("An error occurred while retrieving the inventory stock.", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkInventoryStockResponseDTO>>> GetAllInventoryStocksAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var inventoryStocks = await _inventoryStockRepository.GetAllStockAsync(cancellationToken);
                if(!inventoryStocks.Any()) return BuildBulkSuccessResponse([], "No inventory stocks found.", 200);

                var responseDTOs = inventoryStocks.Select(BuildBulkInventoryStockResponseDTO);
                return BuildBulkSuccessResponse(responseDTOs, "Inventory stocks retrieved successfully.", 200); 
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return BuildBulkErrorResponse("An error occurred while retrieving all inventory stocks.", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkInventoryStockResponseDTO>>> GetInventoryStocksByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                //Validate Product
                var productResult = await GetProductAsync(productId, cancellationToken);
                if(productResult.Error != null) return BuildBulkErrorResponse(productResult.Error.Message!, productResult.Error.StatusCode);

                var inventoryStocks = await _inventoryStockRepository.GetAllStockByProductAsync(productId, cancellationToken);
                if(!inventoryStocks.Any()) return BuildBulkSuccessResponse([], $"No inventory stocks found for Product ID {productId}.", 200);

                var responseDTOs = inventoryStocks.Select(BuildBulkInventoryStockResponseDTO);
                return BuildBulkSuccessResponse(responseDTOs, "Inventory stocks retrieved successfully.", 200); 
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return BuildBulkErrorResponse("An error occurred while retrieving inventory stocks by product ID.", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkInventoryStockResponseDTO>>> GetInventoryStocksByWarehouseIdAsync(int warehouseId, CancellationToken cancellationToken = default)
        {
            try
            {
                //Validate Warehouse
                var warehouseResult = await GetWarehouseAsync(warehouseId, cancellationToken);
                if(warehouseResult.Error != null) return BuildBulkErrorResponse(warehouseResult.Error.Message!, warehouseResult.Error.StatusCode);

                var inventoryStocks = await _inventoryStockRepository.GetAllStockByWarehouseAsync(warehouseId, cancellationToken);
                if(!inventoryStocks.Any()) return BuildBulkSuccessResponse([], $"No inventory stocks found for Warehouse ID {warehouseId}.", 200);

                var responseDTOs = inventoryStocks.Select(BuildBulkInventoryStockResponseDTO);
                return BuildBulkSuccessResponse(responseDTOs, "Inventory stocks retrieved successfully.", 200); 
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return BuildBulkErrorResponse("An error occurred while retrieving inventory stocks by warehouse ID.", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkInventoryStockResponseDTO>>> GetInventoryStocksBelowReorderLevelAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var inventoryStocks = await _inventoryStockRepository.GetStockBelowReorderLevelAsync(cancellationToken);
                if(!inventoryStocks.Any()) return BuildBulkSuccessResponse([], "No inventory stocks found below reorder level.", 200);

                var responseDTOs = inventoryStocks.Select(BuildBulkInventoryStockResponseDTO);
                return BuildBulkSuccessResponse(responseDTOs, "Inventory stocks below reorder level retrieved successfully.", 200);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return BuildBulkErrorResponse("An error occurred while retrieving inventory stocks below reorder level.", 500);
            }
        }

        public async Task<ApiResponse<InventoryStockResponseDTO>> CreateInventoryStockAsync(CreateInventoryStockRequestDTO dto, CancellationToken cancellationToken = default)
        {
            try
            {
                //Validate Product
                var productResult = await GetProductAsync(dto.ProductID, cancellationToken);
                if(productResult.Error != null) return productResult.Error;
                
                //Validate Warehouse
                var warehouseResult = await GetWarehouseAsync(dto.WarehouseID, cancellationToken);
                if(warehouseResult.Error != null) return warehouseResult.Error;

                // Check if the inventory stock already exists for the given product and warehouse
                var existingStock = await _inventoryStockRepository.GetStockByProductAndWarehouseIDAsync(dto.ProductID, dto.WarehouseID, cancellationToken);
                if(existingStock != null) return BuildErrorResponse($"Inventory stock for Product ID {dto.ProductID} and Warehouse ID {dto.WarehouseID} already exists.", 409);

                // Create new inventory stock
                var newStock = new InventoryStock
                {
                    ProductID = dto.ProductID,
                    WarehouseID = dto.WarehouseID,
                    ReorderLevel = dto.ReorderLevel,
                    Quantity = 0, // Initial quantity is set to 0
                    IsActive = true,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                };
                newStock.Product = productResult.Product!;
                newStock.Warehouse = warehouseResult.Warehouse!;
                // Save the new inventory stock to the database
                await _inventoryStockRepository.CreateInventoryStockAsync(newStock, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                // Map to DTO and return success response
                var responseDTO = BuildInventoryStockResponseDTO(newStock);
                return BuildSuccessResponse(responseDTO, "Inventory stock created successfully.", 201);
            }
            catch (DbUpdateException)
            {
                return BuildErrorResponse("Inventory stock already exists for this product and warehouse.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return BuildErrorResponse("An error occurred while creating the inventory stock.", 500);
            }
        }

        public async Task<ApiResponse<InventoryStockResponseDTO>> UpdateReorderLevelAsync(int inventoryStockId, UpdateReorderLevelRequestDTO dto, CancellationToken cancellationToken = default)
        {
            try
            {
                //Validate Inventory Stock
                var stockResult = await GetInventoryStockByIdAsync(inventoryStockId, cancellationToken);
                if(stockResult.Error != null) return stockResult.Error;

                var concurrencyError = ValidateRowVersion(stockResult.Stock!, dto.RowVersion);
                if (concurrencyError != null) return concurrencyError;

                // Update the reorder level
                stockResult.Stock!.ReorderLevel = dto.ReorderLevel;
                stockResult.Stock.Updated = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO and return success response
                var responseDTO = BuildInventoryStockResponseDTO(stockResult.Stock);
                return BuildSuccessResponse(responseDTO, "Reorder level updated successfully.", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BuildErrorResponse("Inventory stock was modified by another request.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return BuildErrorResponse("An error occurred while updating the reorder level.", 500);
            }
        }

        public async Task<ApiResponse<InventoryStockResponseDTO>> ActivateInventoryStockAsync(int inventoryStockId, UpdateInventoryStockStatusRequestDTO dto, CancellationToken cancellationToken = default)
        {
            try
            {
                //Validate Inventory Stock
                var stockResult = await GetInventoryStockByIdAsync(inventoryStockId, cancellationToken);
                if (stockResult.Error != null) return stockResult.Error;

                var concurrencyError = ValidateRowVersion(stockResult.Stock!, dto.RowVersion);
                if (concurrencyError != null) return concurrencyError;
                if (!dto.IsActive) return BuildErrorResponse("IsActive must be true when activating inventory stock.", 400);
                if (stockResult.Stock!.IsActive) return BuildErrorResponse("Inventory stock is already active.", 400);

                // Activate the inventory stock
                stockResult.Stock!.IsActive = true;
                stockResult.Stock.Updated = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO and return success response
                var responseDTO = BuildInventoryStockResponseDTO(stockResult.Stock);
                return BuildSuccessResponse(responseDTO, "Inventory stock activated successfully.", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BuildErrorResponse("Inventory stock was modified by another request.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return BuildErrorResponse("An error occurred while activating the inventory stock.", 500);
            }
        }

        public async Task<ApiResponse<InventoryStockResponseDTO>> DeactivateInventoryStockAsync(int inventoryStockId, UpdateInventoryStockStatusRequestDTO dto, CancellationToken cancellationToken = default)
        {
            try
            {
                //Validate Inventory Stock
                var stockResult = await GetInventoryStockByIdAsync(inventoryStockId, cancellationToken);
                if (stockResult.Error != null) return stockResult.Error;
                var concurrencyError = ValidateRowVersion(stockResult.Stock!, dto.RowVersion);
                if (concurrencyError != null) return concurrencyError;
                if (dto.IsActive) return BuildErrorResponse("IsActive must be false when deactivating inventory stock.", 400);
                if (!stockResult.Stock!.IsActive) return BuildErrorResponse("Inventory stock is already inactive.", 400);
                // Deactivate the inventory stock
                stockResult.Stock!.IsActive = false;
                stockResult.Stock.Updated = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                // Map to DTO and return success response
                var responseDTO = BuildInventoryStockResponseDTO(stockResult.Stock);
                return BuildSuccessResponse(responseDTO, "Inventory stock deactivated successfully.", 200);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BuildErrorResponse("Inventory stock was modified by another request.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return BuildErrorResponse("An error occurred while deactivating the inventory stock.", 500);
            }
        }



        // ==== HELPER METHODS ==== \\
        private async Task<(ApiResponse<InventoryStockResponseDTO>? Error, Product? Product)> GetProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetProductAsync(productId, cancellationToken);

            if(product == null)
            {
                return (new ApiResponse<InventoryStockResponseDTO>
                {
                    Success = false,
                    Message = $"Product with ID {productId} not found.",
                    StatusCode = 404
                }, null);
            }
            return (null, product);
        }
        private async Task<(ApiResponse<InventoryStockResponseDTO>? Error, Warehouse? Warehouse)> GetWarehouseAsync(int warehouseId, CancellationToken cancellationToken = default)
        {
            var warehouse = await _warehouseRepository.GetWarehouseByIdAsync(warehouseId, cancellationToken);
            if(warehouse == null)
            {
                return (new ApiResponse<InventoryStockResponseDTO>
                {
                    Success = false,
                    Message = $"Warehouse with ID {warehouseId} not found.",
                    StatusCode = 404
                }, null);
            }
            return (null, warehouse);
        }
        private async Task<(ApiResponse<InventoryStockResponseDTO>? Error, InventoryStock? Stock)> GetInventoryStockByIdAsync(int stockId, CancellationToken cancellationToken = default)
        {
            var stock = await _inventoryStockRepository.GetStockByIdAsync(stockId, cancellationToken);
            if(stock == null)
            {
                return (new ApiResponse<InventoryStockResponseDTO>
                {
                    Success = false,
                    Message = $"Inventory stock with ID {stockId} not found.",
                    StatusCode = 404
                }, null);
            }
            return (null, stock);
        }


        // BUILDER METHODS \\

        private InventoryStockResponseDTO BuildInventoryStockResponseDTO(InventoryStock stock)
        {
            return new InventoryStockResponseDTO
            {
                ID = stock.ID,
                ProductID = stock.ProductID,
                ProductSku = stock.Product.Sku,
                ProductName = stock.Product.Name,
                WarehouseID = stock.WarehouseID,
                WarehouseName = stock.Warehouse.Name,
                Quantity = stock.Quantity,
                ReorderLevel = stock.ReorderLevel,  
                Created = stock.Created,
                Updated = stock.Updated,
                IsActive = stock.IsActive
                ,RowVersion = stock.RowVersion
            };
        }
        private BulkInventoryStockResponseDTO BuildBulkInventoryStockResponseDTO(InventoryStock stock)
        {
            return new BulkInventoryStockResponseDTO
            {
                ID = stock.ID,
                ProductID = stock.ProductID,
                ProductSku = stock.Product.Sku,
                ProductName = stock.Product.Name,
                WarehouseID = stock.WarehouseID,
                WarehouseName = stock.Warehouse.Name,
                Quantity = stock.Quantity,
                ReorderLevel = stock.ReorderLevel,
                IsBelowReorderLevel = stock.Quantity <= stock.ReorderLevel,
                IsActive = stock.IsActive
            };
        }

        private ApiResponse<InventoryStockResponseDTO> BuildSuccessResponse(InventoryStockResponseDTO data, string message, int statusCode)
        {
            return new ApiResponse<InventoryStockResponseDTO>
            {
                Success = true,
                Data = data,
                Message = message,
                StatusCode = statusCode
            };
        }
        private ApiResponse<IEnumerable<BulkInventoryStockResponseDTO>> BuildBulkSuccessResponse(IEnumerable<BulkInventoryStockResponseDTO> data, string message, int statusCode)
        {
            return new ApiResponse<IEnumerable<BulkInventoryStockResponseDTO>>
            {
                Success = true,
                Data = data,
                Message = message,
                StatusCode = statusCode
            };
        }


        private ApiResponse<InventoryStockResponseDTO> BuildErrorResponse(string message, int statusCode)
        {
            return new ApiResponse<InventoryStockResponseDTO>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode
            };
        }
        private ApiResponse<IEnumerable<BulkInventoryStockResponseDTO>> BuildBulkErrorResponse(string message, int statusCode)
        {
            return new ApiResponse<IEnumerable<BulkInventoryStockResponseDTO>>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode
            };
        }

        private ApiResponse<InventoryStockResponseDTO>? ValidateRowVersion(InventoryStock stock, byte[] rowVersion)
        {
            if (rowVersion.Length == 0)
                return BuildErrorResponse("RowVersion is required.", 400);

            return stock.RowVersion.SequenceEqual(rowVersion)
                ? null
                : BuildErrorResponse("Inventory stock was modified by another request.", 409);
        }
    }
}
