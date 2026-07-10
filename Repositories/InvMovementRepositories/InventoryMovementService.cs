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
                // Fetch the movement from the repository
                var movement = await _movementRepository.GetMovementByIdAsync(movementId);
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
                    ProductId = movement.ProductId,
                    ProductName = movement.Product!.Name,
                    ProductSku = movement.Product.Sku,
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
            // Handle any exceptions that may occur while retrieving the movement
            catch
            {
                return new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to load inventory movement.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetAllMovementsAsync()
        {
            try
            {
                // Fetch all movements from the repository
                var movements = await _movementRepository.GetAllMovementsAsync();

                // Validate if any movements were found
                if (movements == null || !movements.Any())
                {
                    return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                    {
                        Success = false,
                        Message = "No movements found.",
                        StatusCode = 404
                    };
                }

                // Map the movements to the response DTOs
                var movementResponse = movements.Select(m => new BulkInventoryMovementResponseDTO
                {
                    ID = m.ID,
                    ProductId = m.ProductId,
                    ProductName = m.Product!.Name,
                    Quantity = m.Quantity,
                    QuantityBefore = m.QuantityBefore,
                    QuantityAfter = m.QuantityAfter,
                    Movement = m.Movement,
                    UserID = m.UserID,
                    UserName = m.User!.UserName,
                    Reason = m.Reason,
                    Created = m.Created
                });

                // Return the response with the list of movements
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = true,
                    Message = "Movements retrieved successfully.",
                    Data = movementResponse,
                    StatusCode = 200
                };
            }
            // Handle any exceptions that may occur while retrieving all movements
            catch
            {
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to load inventory movements.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetProductMovementHistoryAsync(int productId)
        {
            try
            {
                //Fetch the product from the repository to validate if it exists
                var product = await _productRepository.GetProductAsync(productId);
                if(product == null)
                {
                    return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                    {
                        Success = false,
                        Message = "Product not found.",
                        StatusCode = 404
                    };
                }
                // Fetch the movement history for the specified product
                var movements = await _movementRepository.GetMovementsByProductIdAsync(productId);

                // Validate if any movements were found for the specified product
                if (movements == null || !movements.Any())
                {
                    return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                    {
                        Success = false,
                        Message = "No movement history found for the specified product.",
                        StatusCode = 404
                    };
                }

                // Map the movements to the response DTOs
                var movementResponse = movements.Select(m => new BulkInventoryMovementResponseDTO
                {
                    ID = m.ID,
                    ProductId = m.ProductId,
                    ProductName = m.Product!.Name,
                    Quantity = m.Quantity,
                    QuantityBefore = m.QuantityBefore,
                    QuantityAfter = m.QuantityAfter,
                    Movement = m.Movement,
                    UserID = m.UserID,
                    UserName = m.User!.UserName,
                    Reason = m.Reason,
                    Created = m.Created
                });

                // Return the response with the movement history for the specified product
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = true,
                    Message = "Product movement history retrieved successfully.",
                    Data = movementResponse,
                    StatusCode = 200
                };
            }
            // Handle any exceptions that may occur while retrieving product movement history
            catch
            {
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to load product movement history.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetMovementsByUserIdAsync(int userId)
        {
            try
            {
                // Fetch the user from the repository to validate if it exists
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                    {
                        Success = false,
                        Message = "User not found.",
                        StatusCode = 404
                    };
                }

                // Fetch the movements associated with the specified user ID
                var movements = await _movementRepository.GetMovementsByUserIdAsync(userId);

                // Validate if any movements were found for the specified user
                if (movements == null || !movements.Any())
                {
                    return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                    {
                        Success = false,
                        Message = "No movements found for the specified user.",
                        StatusCode = 404
                    };
                }

                // Map the movements to the response DTOs
                var movementResponse = movements.Select(m => new BulkInventoryMovementResponseDTO
                {
                    ID = m.ID,
                    ProductId = m.ProductId,
                    ProductName = m.Product!.Name,
                    Quantity = m.Quantity,
                    QuantityBefore = m.QuantityBefore,
                    QuantityAfter = m.QuantityAfter,
                    Movement = m.Movement,
                    UserID = m.UserID,
                    UserName = m.User!.UserName,
                    Reason = m.Reason,
                    Created = m.Created
                });

                // Return the response with the movements for the specified user
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = true,
                    Message = "Movements retrieved successfully for the specified user.",
                    Data = movementResponse,
                    StatusCode = 200
                };
            }
            // Handle any exceptions that may occur while retrieving user movement history
            catch
            {
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to load user movement history.",
                    StatusCode = 500
                };
            }
        }
        public async Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Validate the date range
            if (startDate > endDate)
            {
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = false,
                    Message = "Start date cannot be later than end date.",
                    StatusCode = 400
                };
            }
            // Validate that the start date is not in the future
            if (startDate > DateTime.UtcNow)
            {
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = false,
                    Message = "Start date cannot be in the future.",
                    StatusCode = 400
                };
            }
            try
            {
                // Fetch the movements within the specified date range from the repository
                var movements = await _movementRepository.GetMovementsByDateRangeAsync(startDate, endDate);

                // Validate if any movements were found for the specified date range
                if (movements == null || !movements.Any())
                {
                    return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                    {
                        Success = false,
                        Message = "No movements found for the specified date range.",
                        StatusCode = 404
                    };
                }

                // Map the movements to the response DTOs
                var movementResponse = movements.Select(m => new BulkInventoryMovementResponseDTO
                {
                    ID = m.ID,
                    ProductId = m.ProductId,
                    ProductName = m.Product!.Name,
                    Quantity = m.Quantity,
                    QuantityBefore = m.QuantityBefore,
                    QuantityAfter = m.QuantityAfter,
                    Movement = m.Movement,
                    UserID = m.UserID,
                    UserName = m.User!.UserName,
                    Reason = m.Reason,
                    Created = m.Created
                });
                // Return the response with the movements for the specified date range
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = true,
                    Message = "Movements retrieved successfully for the specified date range.",
                    Data = movementResponse,
                    StatusCode = 200
                };
            }
            // Handle any exceptions that may occur while retrieving movements by date range
            catch
            {
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to load movement history by date range.",
                    StatusCode = 500
                };
            }
        }
        public async Task<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>> GetMovementsByMovementTypeAsync(MovementType movementType)
        {
            // Validate the movement type
            if (Enum.IsDefined(typeof(MovementType), movementType) == false)
            {
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = false,
                    Message = "Invalid movement type.",
                    StatusCode = 400
                };
            }
            try
            {
                // Fetch the movements associated with the specified movement type from the repository
                var movements = await _movementRepository.GetMovementsByTypeAsync(movementType);
                if(movements == null || !movements.Any())
                {
                    return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                    {
                        Success = false,
                        Message = "No movements found for the specified movement type.",
                        StatusCode = 404
                    };
                }

                // Map the movements to the response DTOs
                var movementResponse = movements.Select(m => new BulkInventoryMovementResponseDTO
                {
                    ID = m.ID,
                    ProductId = m.ProductId,
                    ProductName = m.Product!.Name,
                    Quantity = m.Quantity,
                    QuantityBefore = m.QuantityBefore,
                    QuantityAfter = m.QuantityAfter,
                    Movement = m.Movement,
                    UserID = m.UserID,
                    UserName = m.User!.UserName,
                    Reason = m.Reason,
                    Created = m.Created
                });

                // Return the response with the movements for the specified movement type
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = true,
                    Message = "Movements retrieved successfully for the specified movement type.",
                    Data = movementResponse,
                    StatusCode = 200
                };
            }
            // Handle any exceptions that may occur while retrieving movements by type
            catch
            {
                return new ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to load movement history by type.",
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
                // Returns ApiResponse if validation not successful
                if(dto.Movement == MovementType.AdjustmentDecrease)
                {
                    var stockValidResult = ValidateStockAvailability(product, dto);
                    if (stockValidResult != null)
                    {
                        return stockValidResult;
                    }
                }
                
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
                    Message = "Internal error occurred, failed to record inventory adjustment.",
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
                    Message = "Internal error occurred, failed to record stock in movement.",
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
                var stockResult = ValidateStockAvailability(product, dto);

                //Return ApiResponse if validation not successful
                if (stockResult != null)
                {
                    return stockResult;
                }

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
                    Message = "Internal error occurred, failed to record stock out movement.",
                    StatusCode = 500
                };
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
            // Validate quantity, cant be less than or equal to zero
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
        /// Validates if the product has sufficient stock for the requested movement.
        /// Only applicable for movements that decrease stock (e.g., StockOut, AdjustmentDecrease).
        /// Returns an ApiResponse with error details if stock is insufficient, otherwise returns null indicating no errors.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        private ApiResponse<InventoryMovementResponseDTO>? ValidateStockAvailability(Product product, CreateInventoryMovementRequestDTO dto)
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
            // If stock is sufficient, return null indicating no errors
            return null;
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
        private async Task<(Product? Product, User? User, ApiResponse<InventoryMovementResponseDTO>? Error)> GetProductAndUserAsync(CreateInventoryMovementRequestDTO dto)
        {
            // Retrieve the product and user from their respective repositories
            var product = await _productRepository.GetProductAsync(dto.ProductId);
            if (product == null)
            {
                // Return an error response if the product is not found
                return (null, null, new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Product not found.",
                    StatusCode = 404
                });
            }
            // Check if the product is active before proceeding
            if (!await _productRepository.IsProductActiveAsync(product.ID))
            {
                // Return an error response if the product is not active
                return (null, null, new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "Product is not active.",
                    StatusCode = 400
                });
            }

            var user = await _userRepository.GetUserByIdAsync(dto.UserID);
            if (user == null)
            {
                // Return an error response if the user is not found
                return (null, null, new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "User not found.",
                    StatusCode = 404
                });
            }
            // Check if the user is active before proceeding
            if (! await _userRepository.IsUserActiveAsync(user.ID))
            {
                // Return an error response if the user is not active
                return (null, null, new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    Message = "User is not active.",
                    StatusCode = 400
                });
            };

            // If both product and user are found and active, return them along with a null error response
            //Return error as null, since both product and user are found successfully, and skip the error handling
            return (product, user, null);
        }


        /// <summary>
        /// Creates an InventoryMovement entity based on the provided DTO and product.
        /// Creates an object of InventoryMovement, which is used for ApiResponse and database update.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="product"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Updates the inventory movement repository and adjusts the product's stock quantity based on the provided movement.
        /// </summary>
        /// <param name="dto">The data transfer object containing inventory movement details.</param>
        /// <param name="movement">The inventory movement entity to be added to the repository.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task UpdateDatabaseAndReturnResponse(CreateInventoryMovementRequestDTO dto, InventoryMovement movement)
        {
            var product = await _productRepository.GetProductAsync(dto.ProductId); // Ensure the product is retrieved before updating stock quantity
            product!.QuantityInStock = movement.QuantityAfter; // Update the product's stock quantity

            await _movementRepository.AddMovementAsync(movement);// Add the inventory movement to the repository
            await _productRepository.SaveChangesAsync(); // Save changes to the product repository
        }


        /// <summary>
        /// Returns an ApiResponse containing the details of the recorded inventory movement.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="movement"></param>
        /// <param name="product"></param>
        /// <param name="user"></param>
        /// <returns></returns>
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
