using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.CoreModels.MovementModels;
using InventoryManagementAPI.Models.CoreModels.UserModels;
using InventoryManagementAPI.Models.DTO_s.MovementDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.InventoryStockRepositories;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using InventoryManagementAPI.Repositories.UserRepositories;
using InventoryManagementAPI.Repositories.WarehouseRepositories;
using Microsoft.EntityFrameworkCore;
using InventoryManagementAPI.Services;

namespace InventoryManagementAPI.Repositories.InvMovementRepositories
{
    public class InventoryMovementService : IInventoryMovementService
    {
        private readonly IInventoryMovementRepository _movementRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IInventoryStockRepository _inventoryStockRepository;
        private readonly IUnitOfWork _unitOfWork;
        public InventoryMovementService(IInventoryMovementRepository movementRepository, IProductRepository productRepository,
            IUserRepository userRepository, IWarehouseRepository warehouseRepository, IInventoryStockRepository inventoryStockRepository, IUnitOfWork unitOfWork)
        {
            _movementRepository = movementRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _warehouseRepository = warehouseRepository;
            _inventoryStockRepository = inventoryStockRepository;
            _unitOfWork = unitOfWork;
        }

        // === GET ===

        public async Task<ApiResponse<InventoryMovementResponseDTO>> GetMovementByIdAsync(int movementId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Fetch the movement from the repository
                var movement = await _movementRepository.GetMovementByIdAsync(movementId, cancellationToken);
                // Validate the movementId
                if (movement == null)
                {
                    return new ApiResponse<InventoryMovementResponseDTO>
                    {
                        Success = false,
                        Message = "Movement not found.",
                        StatusCode = 404
                    };
                }

                // Create a response DTO to return the movement details
                var movementResponse = new InventoryMovementResponseDTO
                {
                    ID = movement.ID,
                    InventoryStockID = movement.InventoryStockID,
                    ProductId = movement.InventoryStock.ProductID,
                    ProductName = movement.InventoryStock.Product.Name,
                    WarehouseID = movement.InventoryStock.WarehouseID,
                    WarehouseName = movement.InventoryStock.Warehouse.Name,
                    Quantity = movement.Quantity,
                    QuantityBefore = movement.QuantityBefore,
                    QuantityAfter = movement.QuantityAfter,
                    Movement = movement.Movement,
                    UserID = movement.UserID,
                    UserName = movement.User!.UserName,
                    Reason = movement.Reason,
                    Created = movement.Created
                };

                // Return the response with the movement details
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = true,
                    Message = "Movement retrieved successfully.",
                    Data = movementResponse,
                    StatusCode = 200
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<InventoryMovementResponseDTO>("Internal error occurred, failed to load inventory movement.", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>> GetAllMovementsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Fetch all movements from the repository
                var movements = await _movementRepository.GetAllMovementsAsync(cancellationToken);

                // Return the response with the list of movements
                return BuildAndReturnBulkGetResponse(movements);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<IEnumerable<InventoryMovementResponseDTO>>("Internal error occurred, failed to load inventory movements.", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>> GetProductMovementHistoryAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                //Fetch the product from the repository to validate if it exists
                var product = await _productRepository.GetProductAsync(productId, cancellationToken);
                if (product == null)
                {
                    return new ApiResponse<IEnumerable<InventoryMovementResponseDTO>>
                    {
                        Success = false,
                        Message = "Product not found.",
                        StatusCode = 404
                    };
                }

                // Fetch the movement history for the specified product
                var movements = await _movementRepository.GetMovementsByProductIdAsync(productId, cancellationToken);

                // Validate if any movements were found for the specified product
                // If movements are found, return a response with the list of movements for the specified product
                return BuildAndReturnBulkGetResponse(movements);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<IEnumerable<InventoryMovementResponseDTO>>("Internal error occurred, failed to load product movement history.", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>> GetMovementsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Fetch the user from the repository to validate if it exists
                var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<InventoryMovementResponseDTO>>
                    {
                        Success = false,
                        Message = "User not found.",
                        StatusCode = 404
                    };
                }
                // Fetch the movements associated with the specified user ID
                var movements = await _movementRepository.GetMovementsByUserIdAsync(userId, cancellationToken);

                // Validate if any movements were found for the specified user
                // If movements are found, return a response with the list of movements for the specified user
                return BuildAndReturnBulkGetResponse(movements);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<IEnumerable<InventoryMovementResponseDTO>>("Internal error occurred, failed to load user movement history.", 500);
            }
        }
        public async Task<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            // Validate the date range
            if (startDate > endDate)
            {
                return new ApiResponse<IEnumerable<InventoryMovementResponseDTO>>
                {
                    Success = false,
                    Message = "Start date cannot be later than end date.",
                    StatusCode = 400
                };
            }
            // Validate that the start date is not in the future
            if (startDate > DateTime.UtcNow)
            {
                return new ApiResponse<IEnumerable<InventoryMovementResponseDTO>>
                {
                    Success = false,
                    Message = "Start date cannot be in the future.",
                    StatusCode = 400
                };
            }
            try
            {
                // Fetch the movements within the specified date range from the repository
                var movements = await _movementRepository.GetMovementsByDateRangeAsync(startDate, endDate, cancellationToken);

                // Validate if any movements were found within the specified date range
                // If movements are found, return a response with the list of movements within the specified date range
                return BuildAndReturnBulkGetResponse(movements);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<IEnumerable<InventoryMovementResponseDTO>>("Internal error occurred, failed to load movement history by date range.", 500);
            }
        }
        public async Task<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>> GetMovementsByMovementTypeAsync(MovementType movementType, CancellationToken cancellationToken = default)
        {
            // Validate the movement type
            if (Enum.IsDefined(typeof(MovementType), movementType) == false)
            {
                return new ApiResponse<IEnumerable<InventoryMovementResponseDTO>>
                {
                    Success = false,
                    Message = "Invalid movement type.",
                    StatusCode = 400
                };
            }
            try
            {
                // Fetch the movements associated with the specified movement type from the repository
                var movements = await _movementRepository.GetMovementsByTypeAsync(movementType, cancellationToken);

                // Validate if any movements were found for the specified movement type
                // If movements are found, return a response with the list of movements for the specified movement type
                return BuildAndReturnBulkGetResponse(movements);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<IEnumerable<InventoryMovementResponseDTO>>("Internal error occurred, failed to load movement history by movement type.", 500);
            }
        }

        // === POST ===

        public async Task<ApiResponse<InventoryMovementResponseDTO>> RecordAdjustmentAsync(CreateInventoryMovementRequestDTO dto, int userId, CancellationToken cancellationToken = default)
        {
            // Assigns results of validationmethod
            var validationResult = RecordValidation(dto);
            if (validationResult != null) return validationResult;

            // Validate movement type for adjustment
            if (dto.Movement != MovementType.AdjustmentIncrease && dto.Movement != MovementType.AdjustmentDecrease)
            {
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Invalid movement type for adjustment.",
                    StatusCode = 400
                };
            }
            try
            {
                //Validate userId Gathered by claims in the controller, to ensure the user exists and is active
                var userValidationResult = await ValidateUserAsync(userId, cancellationToken);
                if (userValidationResult.Error != null) return userValidationResult.Error;

                // Retrieve the product and warehouse based on the provided IDs
                var result = await GetProductAndWarehouseAsync(dto, cancellationToken);
                if (result.Error != null) return result.Error;

                // If both product and warehouse are successfully retrieved, proceed with the adjustment
                // and assign them to local variables for further processing
                var product = result.Product!;
                var warehouse = result.Warehouse!;

                var inventoryStockSearch = await ValidateInventoryStock(product.ID, warehouse.ID, cancellationToken);
                if(inventoryStockSearch.Error != null) return inventoryStockSearch.Error;

                var inventoryStock = inventoryStockSearch.InventoryStock!;


                // Validate stock availability for adjustment decrease
                // Returns ApiResponse if validation not successful
                if (dto.Movement == MovementType.AdjustmentDecrease)
                {
                    var stockValidResult = ValidateStockAvailability(inventoryStock, dto);
                    if (stockValidResult != null) return stockValidResult;
                }

                //Create the InventoryMovement entity
                var movement = CreateInventoryMovement(dto, inventoryStock, userValidationResult.User!);


                // Calculate the updated quantity based on the movement type
                if (dto.Movement == MovementType.AdjustmentIncrease)
                {
                    movement.QuantityAfter = inventoryStock.Quantity + dto.Quantity;
                }
                else if (dto.Movement == MovementType.AdjustmentDecrease)
                {
                    movement.QuantityAfter = inventoryStock.Quantity - dto.Quantity;
                }

                // Update the product's stock quantity and record the movement
                await UpdateDatabaseAndReturnResponse(inventoryStock, movement, cancellationToken);

                // Return the response with the movement details
                return ReturnResponse(movement, inventoryStock);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ApiResponseHelper.Failure<InventoryMovementResponseDTO>("Concurrency error occurred while recording inventory adjustment. Please try again.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<InventoryMovementResponseDTO>("Internal error occurred, failed to record inventory adjustment.", 500);
            }
        }

        public async Task<ApiResponse<InventoryMovementResponseDTO>> RecordStockInAsync(CreateInventoryMovementRequestDTO dto, int userId, CancellationToken cancellationToken = default)
        {
            // Assigns results of validationmethod
            var validationResult = RecordValidation(dto);
            if (validationResult != null) return validationResult;

            // Validate movement type for adjustment
            if (dto.Movement != MovementType.StockIn && dto.Movement != MovementType.Purchase)
            {
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Invalid movement type for adjustment.",
                    StatusCode = 400
                };
            }
            try
            {
                //Validate User
                var userValidationResult = await ValidateUserAsync(userId, cancellationToken);
                if (userValidationResult.Error != null) return userValidationResult.Error;

                // Retrieve the product and warehouse based on the provided IDs
                var result = await GetProductAndWarehouseAsync(dto, cancellationToken);
                if (result.Error != null) return result.Error;

                // If both product and warehouse are successfully retrieved, proceed with the adjustment
                // and assign them to local variables for further processing
                var product = result.Product!;
                var warehouse = result.Warehouse!;

                // Validate inventory stock for the specified product and warehouse
                var inventoryStockSearch = await ValidateInventoryStock(product.ID, warehouse.ID, cancellationToken);
                if(inventoryStockSearch.Error != null) return inventoryStockSearch.Error;

                var inventoryStock = inventoryStockSearch.InventoryStock!;

                //Create the InventoryMovement entity
                var movement = CreateInventoryMovement(dto, inventoryStock, userValidationResult.User!);

                // Calculate the updated quantity based on the movement type
                movement.QuantityAfter = inventoryStock.Quantity + dto.Quantity;

                // Update the product's stock quantity and record the movement
                await UpdateDatabaseAndReturnResponse(inventoryStock, movement, cancellationToken);

                // Return the response with the movement details
                return ReturnResponse(movement, inventoryStock);


            }
            catch (DbUpdateConcurrencyException)
            {
                return ApiResponseHelper.Failure<InventoryMovementResponseDTO>("Concurrency error occurred while recording inventory adjustment. Please try again.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<InventoryMovementResponseDTO>("Internal error occurred, failed to record stock in movement.", 500);
            }
        }

        public async Task<ApiResponse<InventoryMovementResponseDTO>> RecordStockOutAsync(CreateInventoryMovementRequestDTO dto, int userId, CancellationToken cancellationToken = default)
        {
            // Assigns results of validationmethod
            var validationResult = RecordValidation(dto);
            if (validationResult != null) return validationResult;

            // Validate movement type for stock out
            if (dto.Movement != MovementType.StockOut && dto.Movement != MovementType.Sale)
            {
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Invalid movement type for stock out.",
                    StatusCode = 400
                };
            }
            try
            {
                //Validate User
                var userValidationResult = await ValidateUserAsync(userId, cancellationToken);
                if (userValidationResult.Error != null) return userValidationResult.Error;

                // Retrieve the product and warehouse based on the provided IDs
                var result = await GetProductAndWarehouseAsync(dto, cancellationToken);
                if (result.Error != null) return result.Error;

                // If both product and warehouse are successfully retrieved, proceed with the adjustment
                // and assign them to local variables for further processing
                var product = result.Product!;
                var warehouse = result.Warehouse!;

                //Validate inventory stock for the specified product and warehouse
                var inventoryStockSearch = await ValidateInventoryStock(product.ID, warehouse.ID, cancellationToken);
                if(inventoryStockSearch.Error != null) return inventoryStockSearch.Error;

                var inventoryStock = inventoryStockSearch.InventoryStock!;

                // Validate stock availability for adjustment decrease
                var stockResult = ValidateStockAvailability(inventoryStock, dto);
                if (stockResult != null) return stockResult;

                //Create the InventoryMovement entity
                var movement = CreateInventoryMovement(dto, inventoryStock, userValidationResult.User!);

                // Calculate the updated quantity based on the movement type
                movement.QuantityAfter = inventoryStock.Quantity - dto.Quantity;

                // Update the product's stock quantity and record the movement
                await UpdateDatabaseAndReturnResponse(inventoryStock, movement, cancellationToken);

                // Return the response with the movement details
                return ReturnResponse(movement, inventoryStock);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ApiResponseHelper.Failure<InventoryMovementResponseDTO>("Concurrency error occurred while recording inventory adjustment. Please try again.", 409);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return ApiResponseHelper.Failure<InventoryMovementResponseDTO>("Internal error occurred, failed to record stock out movement.", 500);
            }
        }



        // ========================== HELPER METHODS ========================== \\
        /// <summary>
        /// Validates the provided CreateInventoryMovementRequestDTO for recording an inventory movement.
        /// Validates the quantity and reason fields, returning an ApiResponse with error details if validation fails.
        /// Returns null if all validations pass, indicating no errors.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private static ApiResponse<InventoryMovementResponseDTO>? RecordValidation(CreateInventoryMovementRequestDTO dto)
        {
            // Validate quantity, can't be less than or equal to zero
            if (dto.Quantity <= 0)
            {
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Quantity of movement must be greater than zero.",
                    StatusCode = 400
                };
            }

            //Reason validation for adjustment movements
            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Reason for movement is required.",
                    StatusCode = 400
                };
            }
            if (dto.Reason.Length > 500)
            {
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Reason for adjustment cannot exceed 500 characters.",
                    StatusCode = 400
                };
            }

            // If all validations pass, return null indicating no errors
            return null;
        }

        /// <summary>
        /// Validates if the user exists and is active based on the provided userId.
        /// </summary>
        /// <param name="userId">The ID of the user to validate.</param>
        /// <returns>An ApiResponse indicating the validation result, or null if the user is valid.</returns>
        private async Task<(User? User, ApiResponse<InventoryMovementResponseDTO>? Error)> ValidateUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return (null, new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "User not found.",
                    StatusCode = 404
                });
            }
            if (!await _userRepository.IsUserActiveAsync(user.ID, cancellationToken))
            {
                return (null, new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "User is not active.",
                    StatusCode = 400
                });
            }
            return (user, null);
        }

        /// <summary>
        /// Validates if the product has sufficient stock for the requested movement.
        /// Only applicable for movements that decrease stock (e.g., StockOut, AdjustmentDecrease).
        /// Returns an ApiResponse with error details if stock is insufficient, otherwise returns null indicating no errors.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        private ApiResponse<InventoryMovementResponseDTO>? ValidateStockAvailability(InventoryStock stock, CreateInventoryMovementRequestDTO dto)
        {
            if (stock.Quantity < dto.Quantity)
            {
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Insufficient stock for the requested movement.",
                    StatusCode = 400
                };
            }
            // If stock is sufficient, return null indicating no errors
            return null;
        }

        /// <summary>
        /// Validates if the inventory stock exists and is active based on the provided productId and warehouseId.
        /// </summary>
        /// <param name="productId">The ID of the product to validate.</param>
        /// <param name="warehouseId">The ID of the warehouse to validate.</param>
        /// <returns>A tuple containing an ApiResponse indicating the validation result, and the InventoryStock if valid.</returns>
        private async Task<(ApiResponse<InventoryMovementResponseDTO>? Error, InventoryStock? InventoryStock)> ValidateInventoryStock(int productId, int warehouseId, CancellationToken cancellationToken = default)
        {
            var inventoryStock = await _inventoryStockRepository.GetStockByProductAndWarehouseIDAsync(productId, warehouseId, cancellationToken);
            if (inventoryStock == null)
            {
                return (new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Inventory stock not found for the specified product and warehouse.",
                    StatusCode = 404
                }, null);
            }

            if(!inventoryStock.IsActive)
            {
                return (new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Inventory stock is inactive for the specified product and warehouse.",
                    StatusCode = 400
                }, null);
            }

            // If inventory stock is found, return null indicating no errors
            return (null, inventoryStock);
        }

        /// <summary>
        /// Retrieves the product and user based on the provided IDs in the DTO.
        /// The parameter dto is used to extract the ProductId and UserID for retrieval.
        /// Checks if the product and user exist and are active, returning an ApiResponse with error details if either is not found or inactive.
        ///
        /// Returns a tuple containing the retrieved Product, User, and an optional ApiResponse for error handling.
        /// Returns null for Product and User if either is not found, along with an error response in the ApiResponse.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private async Task<(Product? Product, Warehouse? Warehouse, ApiResponse<InventoryMovementResponseDTO>? Error)> GetProductAndWarehouseAsync(CreateInventoryMovementRequestDTO dto, CancellationToken cancellationToken = default)
        {
            // Retrieve the product and user from their respective repositories
            var product = await _productRepository.GetProductAsync(dto.ProductID, cancellationToken);
            if (product == null)
            {
                // Return an error response if the product is not found
                return (null, null, ApiResponseHelper.Failure<InventoryMovementResponseDTO>($"Product with ID {dto.ProductID} not found.", 404));
            }
            // Check if the product is active before proceeding
            if (!await _productRepository.IsProductActiveAsync(product.ID, cancellationToken))
            {
                // Return an error response if the product is not active
                return (null, null, ApiResponseHelper.Failure<InventoryMovementResponseDTO>($"Product with ID {dto.ProductID} is not active.", 400));
            }

            var warehouse = await _warehouseRepository.GetWarehouseByIdAsync(dto.WarehouseID, cancellationToken);
            if (warehouse == null)
            {
                // Return an error response if the warehouse is not found
                return (null, null, ApiResponseHelper.Failure<InventoryMovementResponseDTO>($"Warehouse with ID {dto.WarehouseID} not found.", 404));
            }
            // Check if the warehouse is active before proceeding
            if (!await _warehouseRepository.IsWarehouseActiveAsync(warehouse.ID, cancellationToken))
            {
                // Return an error response if the warehouse is not active
                return (null, null, ApiResponseHelper.Failure<InventoryMovementResponseDTO>($"Warehouse with ID {dto.WarehouseID} is not active.", 400));
            }

            // If both product and user are found and active, return them along with a null error response
            //Return error as null, since both product and user are found successfully, and skip the error handling
            return (product, warehouse, null);
        }


        /// <summary>
        /// Creates an InventoryMovement entity based on the provided DTO and product.
        /// Creates an object of InventoryMovement, which is used for ApiResponse and database update.
        /// </summary>
        /// <param name="dto">The DTO containing the details for the inventory movement.</param>
        /// <param name="inventoryStock">The inventory stock associated with the movement.</param>
        /// <param name="user">The user performing the movement.</param>
        /// <returns></returns>
        private InventoryMovement CreateInventoryMovement(CreateInventoryMovementRequestDTO dto, InventoryStock inventoryStock, User user)
        {
            return new InventoryMovement
            {
                InventoryStockID = inventoryStock.ID,
                UserID = user.ID,
                User = user,
                Quantity = dto.Quantity,
                QuantityBefore = inventoryStock.Quantity,
                Movement = dto.Movement,
                Reason = dto.Reason,
                Created = DateTime.UtcNow,
            };
        }


        /// <summary>
        /// Updates the inventory stock quantity and records the inventory movement in the database.
        /// </summary>
        /// <param name="inventoryStock">The inventory stock to update.</param>
        /// <param name="movement">The inventory movement to record.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task UpdateDatabaseAndReturnResponse(InventoryStock inventoryStock, InventoryMovement movement, CancellationToken cancellationToken = default)
        {
            inventoryStock.Quantity = movement.QuantityAfter; // Update the inventory stock quantity

            await _movementRepository.AddMovementAsync(movement, cancellationToken);// Add the inventory movement to the repository
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }


        /// <summary>
        /// Returns an ApiResponse containing the details of the recorded inventory movement.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="movement"></param>
        /// <param name="product"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private ApiResponse<InventoryMovementResponseDTO> ReturnResponse(InventoryMovement movement, InventoryStock inventoryStock)
        {
            return new ApiResponse<InventoryMovementResponseDTO>
            {
                Success = true,
                Message = "Movement recorded successfully.",
                Data = new InventoryMovementResponseDTO
                {
                    ID = movement.ID,
                    ProductId = inventoryStock.Product.ID,
                    ProductName = inventoryStock.Product.Name,
                    WarehouseID = inventoryStock.WarehouseID,
                    WarehouseName = inventoryStock.Warehouse.Name,
                    Quantity = movement.Quantity,
                    QuantityBefore = movement.QuantityBefore,
                    QuantityAfter = movement.QuantityAfter,
                    Movement = movement.Movement,
                    UserID = movement.UserID,
                    UserName = movement.User.UserName,
                    Reason = movement.Reason,
                    Created = movement.Created
                },
                StatusCode = 201
            };
        }

        /// <summary>
        /// Builds and returns an ApiResponse containing a list of InventoryMovementResponseDTOs based on the provided movements.
        /// </summary>
        /// <param name="movements">The list of inventory movements to include in the response.</param>
        /// <returns>An ApiResponse containing the list of InventoryMovementResponseDTOs.</returns>
        private ApiResponse<IEnumerable<InventoryMovementResponseDTO>> BuildAndReturnBulkGetResponse(IEnumerable<InventoryMovement> movements)
        {
            // Validate if any movements were found
            if (movements == null || !movements.Any())
            {
                return ApiResponseHelper.Failure<IEnumerable<InventoryMovementResponseDTO>>("No inventory movements found.", 404);
            }

            // Map the movements to the response DTOs
            var movementResponse = movements.Select(m => new InventoryMovementResponseDTO
            {
                ID = m.ID,
                InventoryStockID = m.InventoryStockID,

                ProductId = m.InventoryStock.ProductID,
                ProductName = m.InventoryStock.Product.Name,

                WarehouseID = m.InventoryStock.WarehouseID,
                WarehouseName = m.InventoryStock.Warehouse.Name,

                Quantity = m.Quantity,
                QuantityBefore = m.QuantityBefore,
                QuantityAfter = m.QuantityAfter,

                Movement = m.Movement,

                UserID = m.UserID,
                UserName = m.User.UserName,

                Reason = m.Reason,
                Created = m.Created
            });

            // Return the response with the list of movements
            return ApiResponseHelper.Success<IEnumerable<InventoryMovementResponseDTO>>(movementResponse, "Movements retrieved successfully.", 200);
        }
    }
}
