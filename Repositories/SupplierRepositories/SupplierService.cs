using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierDTO_s;
using System.Net.Mail;

namespace InventoryManagementAPI.Repositories.SupplierRepositories
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierService(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        // === GET === \\
        public async Task<ApiResponse<IEnumerable<SupplierResponseDTO>>> GetAllSuppliersAsync()
        {
            try
            {
                // Retrieve all suppliers from the repository
                var supplierList = await _supplierRepository.GetAllSuppliersAsync();

                // Return not found if no suppliers exist
                if (supplierList == null || !supplierList.Any())
                {
                    return new ApiResponse<IEnumerable<SupplierResponseDTO>>
                    {
                        Success = false,
                        Message = "No Suppliers Found",
                        StatusCode = 404
                    };
                }
                // Build the response DTO list from the supplier entities
                List<SupplierResponseDTO> supplierDtoList = new List<SupplierResponseDTO>();

                foreach (var supplier in supplierList)
                {
                    var supplierDto = new SupplierResponseDTO
                    {
                        ID = supplier.ID,
                        Name = supplier.Name,
                        ContactName = supplier.ContactName,
                        PhoneContact = supplier.PhoneContact,
                        EmailContact = supplier.EmailContact,
                        IsActive = supplier.IsActive
                    };
                    supplierDtoList.Add(supplierDto);
                }
                // Return the supplier list in the ApiResponse
                return new ApiResponse<IEnumerable<SupplierResponseDTO>>
                {
                    Success = true,
                    Data = supplierDtoList,
                    Message = "Suppliers successfully retrieved",
                    StatusCode = 200
                };
            }
            // Handle any exceptions that may occur while retrieving suppliers
            catch
            {
                return new ApiResponse<IEnumerable<SupplierResponseDTO>>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to load suppliers.",
                    StatusCode = 500
                };
            }
        }
        public async Task<ApiResponse<SupplierResponseDTO>> GetSupplierByIdAsync(int supplierId)
        {
            try
            {
                // Retrieve the supplier by ID
                var supplierExists = await FindSupplierById(supplierId);
                if (supplierExists.Supplier == null)
                {
                    // Return the error response if the supplier does not exist
                    return supplierExists.Error!;
                }

                //Assign the existing supplier entity to a variable for building the response DTO
                var supplier = supplierExists.Supplier;

                // Build and return the response DTO from the supplier entity
                return BuildSupplierResponseDTO(supplier, "Supplier retrieved successfully.", 200);
            }
            catch
            {
                return BuildCatchErrorResponse("Internal error occurred, failed to retrieve supplier.");
            }
        }


        // === POST === \\
        public async Task<ApiResponse<SupplierResponseDTO>> CreateSupplierAsync(CreateSupplierRequestDTO supplier)
        {
            // Validate that the request body was supplied
            var dtoValidationResult = ValidateDto(supplier);
            if (dtoValidationResult != null)
            {
                // Return the validation error response if the DTO is null
                return dtoValidationResult;
            }

            // Validate the supplier DTO for required fields and correct formats
            var validationResult = ValidateDtoFields(supplier.Name, supplier.ContactName, supplier.EmailContact, supplier.PhoneContact, supplier.Address);
            if (validationResult != null)
            {
                // Return the validation error response if any required fields are missing or incorrectly formatted
                return validationResult;
            }

            try
            {
                // Validate that the supplier name and email do not already exist
                var nameEmailCheckResult = await CheckNameEmailExistsAdd(supplier.Name, supplier.EmailContact);
                if (nameEmailCheckResult != null)
                {
                    return nameEmailCheckResult;
                }

                // Create the supplier entity from the request DTO
                var newSupplier = new Supplier
                {
                    Name = supplier.Name,
                    ContactName = supplier.ContactName,
                    PhoneContact = supplier.PhoneContact,
                    EmailContact = supplier.EmailContact,
                    Address = supplier.Address,
                    IsActive = supplier.IsActive,
                    Created = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                // Save the supplier to the repository
                var createdSupplier = await _supplierRepository.CreateSupplierAsync(newSupplier);

                return BuildSupplierResponseDTO(createdSupplier, "Supplier successfully created", 201);
            }
            catch
            {
                return BuildCatchErrorResponse("Internal error occurred, failed to create supplier.");
            }
        }


        // === PUT === \\
        public async Task<ApiResponse<SupplierResponseDTO>> UpdateSupplierAsync(int supplierId, UpdateSupplierRequestDTO updateSupplierDTO)
        {
            //Validate that the request body was supplied
            var dtoValidationResult = ValidateDto(updateSupplierDTO);
            if (dtoValidationResult != null)
            {
                // Return the validation error response if the DTO is null
                return dtoValidationResult;
            }

            // Validate the supplier DTO for required fields and correct formats
            var validationResult = ValidateDtoFields(updateSupplierDTO.Name, updateSupplierDTO.ContactName, updateSupplierDTO.EmailContact, updateSupplierDTO.PhoneContact, updateSupplierDTO.Address);
            if (validationResult != null)
            {
                // Return the validation error response if any required fields are missing or incorrectly formatted
                return validationResult;
            }
            try
            {
                // Validate that the supplier ID exists and that the name and email do not already exist for another supplier
                var validateAgainstExisting = await CheckIdNameEmailExistsUpdate(supplierId, updateSupplierDTO.Name, updateSupplierDTO.EmailContact);
                if (validateAgainstExisting.Supplier == null)
                {
                    return validateAgainstExisting.Error!;
                }

                //Assign the existing supplier entity to a variable for updating
                var supplier = validateAgainstExisting.Supplier;

                // Apply the updateSupplierDTO values to the supplier entity
                supplier.Name = updateSupplierDTO.Name;
                supplier.ContactName = updateSupplierDTO.ContactName;
                supplier.PhoneContact = updateSupplierDTO.PhoneContact;
                supplier.EmailContact = updateSupplierDTO.EmailContact;
                supplier.Address = updateSupplierDTO.Address;
                supplier.IsActive = updateSupplierDTO.IsActive;
                supplier.LastUpdated = DateTime.UtcNow;

                // Save the updated supplier through the repository
                await _supplierRepository.UpdateSupplierAsync(supplier);

                // Return the updated supplier details in the response
                return BuildSupplierResponseDTO(supplier, "Supplier details successfully updated", 200);
            }
            catch
            {
                return BuildCatchErrorResponse("Internal error occurred, failed to update supplier.");
            }
        }

        // === SET ACTIVE STATUS === \\
        public async Task<ApiResponse<SupplierResponseDTO>> ActivateSupplierAsync(int supplierId)
        {
            try
            {
                // Retrieve the supplier before attempting to activate it
                var supplierExistsCheck = await FindSupplierById(supplierId);

                if (supplierExistsCheck.Supplier == null)
                {
                    // Return the error response if the supplier does not exist
                    return supplierExistsCheck.Error!;
                }

                // Assign the existing supplier entity to a variable for updating
                var supplier = supplierExistsCheck.Supplier;

                // Return a bad request response if the supplier is already active
                if (supplier.IsActive)
                {
                    return new ApiResponse<SupplierResponseDTO>
                    {
                        Success = false,
                        Message = "Supplier is already active",
                        StatusCode = 400
                    };
                }

                // Set the supplier active and update the timestamp
                supplier.IsActive = true;
                supplier.LastUpdated = DateTime.UtcNow;
                await _supplierRepository.SaveChangesAsync();

                // Return the activated supplier details
                return BuildSupplierResponseDTO(supplier, "Supplier successfully activated", 200);
            }
            catch
            {
                return BuildCatchErrorResponse("Internal error occurred, failed to activate supplier.");
            }
        }

        public async Task<ApiResponse<SupplierResponseDTO>> DeactivateSupplierAsync(int supplierId)
        {
            try
            {
                // Retrieve the supplier before attempting to deactivate it
                var supplierExistsCheck = await FindSupplierById(supplierId);

                if (supplierExistsCheck.Supplier == null)
                {
                    // Return the error response if the supplier does not exist
                    return supplierExistsCheck.Error!;
                }

                // Assign the existing supplier entity to a variable for updating
                var supplier = supplierExistsCheck.Supplier;


                // Return a bad request response if the supplier is already inactive
                if (!supplier.IsActive)
                {
                    return new ApiResponse<SupplierResponseDTO>
                    {
                        Success = false,
                        Message = "Supplier is already inactive",
                        StatusCode = 400
                    };
                }

                // Set the supplier inactive and update the timestamp
                supplier.IsActive = false;
                supplier.LastUpdated = DateTime.UtcNow;
                await _supplierRepository.SaveChangesAsync();

                // Return the deactivated supplier details Api Response
                return BuildSupplierResponseDTO(supplier, "Supplier successfully deactivated", 200);
            }
            catch
            {
                return BuildCatchErrorResponse("Internal error occurred, failed to deactivate supplier.");
            }
        }


        // === FIND SUPPLIER BY ID HELPER METHOD === \\

        /// <summary>
        /// Finds a supplier by ID.
        /// </summary>
        /// <param name="supplierId"></param>
        /// <returns>
        /// Returns a tuple containing the Supplier entity if found, and an ApiResponse with an error message if not found.
        /// </returns>
        private async Task<(Supplier? Supplier, ApiResponse<SupplierResponseDTO>? Error)> FindSupplierById(int supplierId)
        {
            var supplier = await _supplierRepository.GetSupplierByIdAsync(supplierId);
            if (supplier == null)
            {
                return (null, new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Supplier not found",
                    StatusCode = 404
                });
            }
            return (supplier, null);
        }


        // === BUILD RESPONSE METHODS === \\

        /// <summary>
        /// Builds an ApiResponse containing a SupplierResponseDTO from a Supplier entity, along with a message and status code.
        /// </summary>
        /// <param name="supplier"></param>
        /// <param name="message"></param>
        /// <param name="statusCode"></param>
        /// <returns>
        /// Returns an ApiResponse containing the SupplierResponseDTO, success status, message, and status code.
        /// </returns>
        private ApiResponse<SupplierResponseDTO> BuildSupplierResponseDTO(Supplier supplier, string message, int statusCode)
        {
            var supplierResponse = new SupplierResponseDTO
            {
                ID = supplier.ID,
                Name = supplier.Name,
                ContactName = supplier.ContactName,
                PhoneContact = supplier.PhoneContact,
                EmailContact = supplier.EmailContact,
                IsActive = supplier.IsActive
            };
            return new ApiResponse<SupplierResponseDTO>
            {
                Success = true,
                Data = supplierResponse,
                Message = message,
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Builds an ApiResponse for error handling.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>
        /// Returning a failure response with a message and status code 500.
        /// </returns>
        private ApiResponse<SupplierResponseDTO> BuildCatchErrorResponse(string message)
        {
            return new ApiResponse<SupplierResponseDTO>
            {
                Success = false,
                Message = message,
                StatusCode = 500
            };
        }


        // === VALIDATION HELPER METHODS=== \\

        /// <summary>
        /// Validates DTO for a null value.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>
        /// Returns an ApiResponse with a validation error if the DTO is null, or null if the DTO is valid.
        /// </returns>
        private static ApiResponse<SupplierResponseDTO>? ValidateDto(object? dto)
        {
            // Validate that the request body was supplied
            if (dto == null)
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Invalid supplier object model",
                    StatusCode = 400
                };
            }
            return null;
        }

        /// <summary>
        /// Validates the supplier DTO for required fields and correct formats. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contactName"></param>
        /// <param name="emailContact"></param>
        /// <param name="phoneContact"></param>
        /// <param name="address"></param>
        /// <returns>
        /// Returns an ApiResponse with validation errors if any, or null if validation passes.
        /// </returns>
        private static ApiResponse<SupplierResponseDTO>? ValidateDtoFields(string name, string contactName, string emailContact, string phoneContact, string address)
        {
            // Validate required supplier name
            if (string.IsNullOrWhiteSpace(name))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Provide a supplier Name",
                    StatusCode = 400
                };
            }
            // Validate required contact name
            if (string.IsNullOrWhiteSpace(contactName))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Provide a supplier  contact Name",
                    StatusCode = 400
                };
            }
            // Validate basic supplier email format
            if (string.IsNullOrWhiteSpace(emailContact) || !IsValidEmail(emailContact))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Provide a correct supplier email",
                    StatusCode = 400
                };
            }
            // Validate supplier phone length and required value
            if (string.IsNullOrWhiteSpace(phoneContact) || phoneContact.Length < 4 || phoneContact.Length > 20)
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Provide a correct supplier phone",
                    StatusCode = 400
                };
            }
            // Validate required supplier address
            if (string.IsNullOrWhiteSpace(address))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Provide a supplier address",
                    StatusCode = 400
                };
            }
            return null;
        }

        /// <summary>
        /// Checks if the supplier name or email already exists in the repository when adding a new supplier.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <returns>
        /// Returns an ApiResponse with validation errors if the name or email already exists, or null if both are unique.
        /// </returns>
        private async Task<ApiResponse<SupplierResponseDTO>?> CheckNameEmailExistsAdd(string name, string email)
        {
            // Validate that the supplier name does not already exist
            if (await _supplierRepository.SupplierNameExistsAsync(name))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Supplier Name already exists",
                    StatusCode = 400
                };
            }
            // Validate that the supplier email does not already exist
            if (await _supplierRepository.SupplierEmailExistsAsync(email))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Supplier Email already exists",
                    StatusCode = 400
                };
            }
            return null;
        }

        /// <summary>
        /// Validates that the supplier ID exists and that the name and email do not already exist for another supplier when updating an existing supplier.
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <returns>
        /// Returns a tuple containing the existing supplier entity and an ApiResponse with validation errors if any, or null if validation passes.
        /// </returns>
        private async Task<(Supplier? Supplier, ApiResponse<SupplierResponseDTO>? Error)> CheckIdNameEmailExistsUpdate(int supplierId, string name, string email)
        {
            // Retrieve the supplier before applying updates
            var supplierExistsCheck = await FindSupplierById(supplierId);
            if (supplierExistsCheck.Supplier == null)
            {
                // Return the error API response if the supplier does not exist
                return (null, supplierExistsCheck.Error);
            }

            // Validate that the supplier name does not already exist for another supplier
            if (await _supplierRepository.SupplierNameExistsForOtherSupplierAsync(supplierId, name))
            {
                return (null, new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Supplier Name already exists",
                    StatusCode = 400
                });
            }
            //Validate that the supplier email does not already exist for another supplier
            if (await _supplierRepository.SupplierEmailExistsForOtherSupplierAsync(supplierId, email))
            {
                return (null, new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Supplier Email already exists",
                    StatusCode = 400
                });
            }
            return (supplierExistsCheck.Supplier, null);
        }

        /// <summary>
        /// Validates the email format using the System.Net.Mail.MailAddress class. Returns true if the email is valid, false otherwise.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>
        /// Returns true if the email is valid, false otherwise.
        /// </returns>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
