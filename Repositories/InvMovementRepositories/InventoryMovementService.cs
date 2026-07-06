using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.MovementDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.ProductRepositorys;
using InventoryManagementAPI.Repositories.UserRepositories;

namespace InventoryManagementAPI.Repositories.InvMovementRepositories
{
    public class InventoryMovementService : IInventoryMovementService
    {
        private readonly IInventoryMovementRepository _movementRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        public InventoryMovementService(IInventoryMovementRepository movementRepository, IProductRepository productRepository, IUserRepository userRepository)
        {
            _movementRepository = movementRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }
        
        // === GET === \\

        public async Task<ApiResponse<InventoryMovementResponseDTO>> GetMovementByIdAsync(int movementId)
        {
            try
            {
                var movement = await _movementRepository.GetMovementByIdAsync(movementId);
                if (movement == null)
                {
                    return new ApiResponse<InventoryMovementResponseDTO>
                    {
                        Success = false,
                        Message = "Movement not found.",
                        StatusCode = 404
                    };
                }
                var movementResponse = new InventoryMovementResponseDTO
                {
                    ID = movement.ID,
                    ProductId = movement.ProductId,
                    ProductName = movement.Product.Name,
                    ProductSku = movement.Product.Sku,
                    Quantity = movement.Quantity,
                    QuantityBefore = movement.QuantityBefore,
                    QuantityAfter = movement.QuantityAfter,
                    Movement = movement.Movement,
                    UserID = movement.UserID,
                    UserName = movement.User.UserName,
                    Reason = movement.Reason,
                    Created = movement.Created
                };
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = true,
                    Message = "Movement retrieved successfully.",
                    Data = movementResponse,
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the movement.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetAllMovementsAsync()
        {
            try
            {
                var movements = await _movementRepository.GetAllMovementsAsync();

                if (movements == null)
                {
                    return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                    {
                        Success = false,
                        Message = "No movements found.",
                        StatusCode = 404
                    };
                }
                var movementResponse = movements.Select(m => new BulkInventoryMovementResponseDTO
                {
                    ID = m.ID,
                    ProductId = m.ProductId,
                    ProductName = m.Product.Name,
                    Quantity = m.Quantity,
                    QuantityBefore = m.QuantityBefore,
                    QuantityAfter = m.QuantityAfter,
                    Movement = m.Movement,
                    UserID = m.UserID,
                    Reason = m.Reason,
                    Created = m.Created
                });

                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = true,
                    Message = "Movements retrieved successfully.",
                    Data = movementResponse,
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving movements.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetProductMovementHistoryAsync(int productId)
        {
            try
            {
                var movements = await _movementRepository.GetMovementsByProductIdAsync(productId);
                if (movements == null || !movements.Any())
                {
                    return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                    {
                        Success = false,
                        Message = "No movement history found for the specified product.",
                        StatusCode = 404
                    };
                }

                var movementResponse = movements.Select(m => new BulkInventoryMovementResponseDTO
                {
                    ID = m.ID,
                    ProductId = m.ProductId,
                    ProductName = m.Product.Name,
                    Quantity = m.Quantity,
                    QuantityBefore = m.QuantityBefore,
                    QuantityAfter = m.QuantityAfter,
                    Movement = m.Movement,
                    UserID = m.UserID,
                    Reason = m.Reason,
                    Created = m.Created
                });

                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = true,
                    Message = "Product movement history retrieved successfully.",
                    Data = movementResponse,
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving product movement history.",
                    StatusCode = 500
                };
            }
        }



        // === POST === \\

        public async Task<ApiResponse<InventoryMovementResponseDTO>> RecordAdjustmentAsync(CreateInventoryMovementRequestDTO dto)
        {
            // Assigns results of validationmethod
            var validationResult = RecordValidation(dto);
            // If validation does succeed, will skip if statement
            // If validation does not suceed returns ApiResponse from RecordValidation method
            if (validationResult != null)
            {
                return validationResult;
            }

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
                // Retrieve the product and user based on the provided IDs
                var result = await GetProductAndUserAsync(dto);

                // Check if there was an error in retrieving the product or user
                if (result.Error != null)
                {
                    return result.Error;
                }

                // If both product and user are successfully retrieved, proceed with the adjustment
                // and assign them to local variables for further processing
                var product = result.Product!;
                var user = result.User!;

                // Validate stock availability for adjustment decrease
                ValidateStockAvailability(product, dto);

                //Create the InventoryMovement entity
                var movement = CreateInventoryMovement(dto, product);


                // Calculate the updated quantity based on the movement type
                if (dto.Movement == MovementType.AdjustmentIncrease)
                {
                    movement.QuantityAfter = product.QuantityInStock + dto.Quantity;
                }
                else if (dto.Movement == MovementType.AdjustmentDecrease)
                {
                    movement.QuantityAfter = product.QuantityInStock - dto.Quantity;
                }

                // Update the product's stock quantity and record the movement
                await UpdateDatabaseAndReturnResponse(dto, movement);

                // Return the response with the movement details
                return ReturnResponse(dto, movement, product, user);
            }
            catch
            {
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred while recording the adjustment.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<InventoryMovementResponseDTO>> RecordStockInAsync(CreateInventoryMovementRequestDTO dto)
        {
            // Assigns results of validationmethod
            var validationResult = RecordValidation(dto);
            // If validation does succeed, will skip if statement
            // If validation does not suceed returns ApiResponse from RecordValidation method
            if (validationResult != null)
            {
                return validationResult;
            }

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
                // Retrieve the product and user based on the provided IDs
                var result = await GetProductAndUserAsync(dto);

                // Check if there was an error in retrieving the product or user
                if (result.Error != null)
                {
                    return result.Error;
                }

                // If both product and user are successfully retrieved, proceed with the adjustment
                // and assign them to local variables for further processing
                var product = result.Product!;
                var user = result.User!;

                //Create the InventoryMovement entity
                var movement = CreateInventoryMovement(dto, product);

                // Calculate the updated quantity based on the movement type
                movement.QuantityAfter = product.QuantityInStock + dto.Quantity;

                // Update the product's stock quantity and record the movement
                await UpdateDatabaseAndReturnResponse(dto, movement);

                // Return the response with the movement details
                return ReturnResponse(dto, movement, product, user);


            }
            catch
            {
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred while recording the stock in movement.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<InventoryMovementResponseDTO>> RecordStockOutAsync(CreateInventoryMovementRequestDTO dto)
        {
            // Assigns results of validationmethod
            var validationResult = RecordValidation(dto);
            // If validation does succeed, will skip if statement
            // If validation does not suceed returns ApiResponse from RecordValidation method
            if (validationResult != null)
            {
                return validationResult;
            }

            try
            {
                // Retrieve the product and user based on the provided IDs
                var result = await GetProductAndUserAsync(dto);

                // Check if there was an error in retrieving the product or user
                if (result.Error != null)
                {
                    return result.Error;
                }

                // If both product and user are successfully retrieved, proceed with the adjustment
                // and assign them to local variables for further processing
                var product = result.Product!;
                var user = result.User!;

                // Validate stock availability for adjustment decrease
                ValidateStockAvailability(product, dto);

                //Create the InventoryMovement entity
                var movement = CreateInventoryMovement(dto, product);

                // Calculate the updated quantity based on the movement type
                movement.QuantityAfter = product.QuantityInStock - dto.Quantity;

                // Update the product's stock quantity and record the movement
                await UpdateDatabaseAndReturnResponse(dto, movement);

                // Return the response with the movement details
                return ReturnResponse(dto, movement, product, user);
            }
            catch
            {
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred while recording the stock in movement.",
                    StatusCode = 500
                };
            }
        }



        // ========================== HELPER METHODS ========================== \\

        private static ApiResponse<InventoryMovementResponseDTO> RecordValidation(CreateInventoryMovementRequestDTO dto)
        {
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
                    Message = "Reason for adjustment is required.",
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

        private ApiResponse<InventoryMovementResponseDTO> ValidateStockAvailability(Product product, CreateInventoryMovementRequestDTO dto)
        {
            if (product.QuantityInStock < dto.Quantity)
            {
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Insufficient stock for the requested movement.",
                    StatusCode = 400
                };
            }
            return null;
        }

        private async Task<(Product? Product, User? User, ApiResponse<InventoryMovementResponseDTO>? Error)> GetProductAndUserAsync(CreateInventoryMovementRequestDTO dto)
        {
            if (dto.ProductId <= 0)
            {

                return (null, null, new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Invalid product ID.",
                    StatusCode = 400
                });
            }

            if (dto.UserID <= 0)
            {

                return (null, null, new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Invalid product ID.",
                    StatusCode = 400
                });
            }

            var product = await _productRepository.GetProductAsync(dto.ProductId);
            if (product == null)
            {
                return (null, null, new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Product not found.",
                    StatusCode = 404
                });
            }

            var user = await _userRepository.GetUserByIdAsync(dto.UserID);
            if (user == null)
            {
                return (null, null, new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "User not found.",
                    StatusCode = 404
                });
            }

            return (product, user, null);
        }

        private InventoryMovement CreateInventoryMovement(CreateInventoryMovementRequestDTO dto, Product product)
        {
            return new InventoryMovement
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                QuantityBefore = product.QuantityInStock,
                Movement = dto.Movement,
                UserID = dto.UserID,
                Reason = dto.Reason,
                Created = DateTime.UtcNow,
            };
        }

        private async Task UpdateDatabaseAndReturnResponse(CreateInventoryMovementRequestDTO dto, InventoryMovement movement)
        {
            await _movementRepository.AddMovementAsync(movement);

            var product = await _productRepository.GetProductAsync(dto.ProductId); // Ensure the product is retrieved before updating stock quantity
            product.QuantityInStock = movement.QuantityAfter; // Update the product's stock quantity
            await _productRepository.SaveChangesAsync(); // Save changes to the product repository
        }

        private ApiResponse<InventoryMovementResponseDTO> ReturnResponse(CreateInventoryMovementRequestDTO dto, InventoryMovement movement, Product product, User user)
        {
            return new ApiResponse<InventoryMovementResponseDTO>
            {
                Success = true,
                Message = "Movement recorded successfully.",
                Data = new InventoryMovementResponseDTO
                {
                    ID = movement.ID,
                    ProductId = movement.ProductId,
                    ProductName = product.Name,
                    ProductSku = product.Sku,
                    Quantity = movement.Quantity,
                    QuantityBefore = movement.QuantityBefore,
                    QuantityAfter = movement.QuantityAfter,
                    Movement = movement.Movement,
                    UserID = movement.UserID,
                    UserName = user.UserName,
                    Reason = movement.Reason,
                    Created = movement.Created
                },
                StatusCode = 201
            };
        }
    }
}
