using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierDTO_s;

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

                foreach(var supplier in supplierList)
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
                    Message = "Suppliers Succesffuly retrieved",
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
            // Validate the supplier ID before querying the repository
            if(supplierId <= 0)
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Invalid Supplier ID",
                    StatusCode = 400
                };
            }
            try
            {
                // Retrieve the supplier by ID
                var supplier = await _supplierRepository.GetSupplierByIdAsync(supplierId);
                if(supplier == null)
                {
                    return new ApiResponse<SupplierResponseDTO>
                    {
                        Success = false,
                        Message = "Supplier not found",
                        StatusCode = 404
                    };
                }

                // Map the supplier entity to the response DTO
                var supplierResponse = new SupplierResponseDTO
                {
                    ID = supplier.ID,
                    Name = supplier.Name,
                    ContactName = supplier.ContactName,
                    PhoneContact = supplier.PhoneContact,
                    EmailContact = supplier.EmailContact,
                    IsActive = supplier.IsActive
                };

                // Return the supplier details in the ApiResponse
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = true,
                    Data = supplierResponse,
                    Message = "Supplier Succesffuly Retrieved",
                    StatusCode = 200
                };
            }
            // Handle any exceptions that may occur while retrieving the supplier
            catch
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to load supplier.",
                    StatusCode = 500
                };
            }
        }


        // === POST === \\
        public async Task<ApiResponse<SupplierResponseDTO>> CreateSupplierAsync(CreateSupplierRequestDTO supplier)
        {
            // Validate that the request body was supplied
            if(supplier == null)
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Supplier Not Created",
                    StatusCode = 400
                };
            }
            // Validate required supplier name
            if (String.IsNullOrEmpty(supplier.Name))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Provide a supplier Name",
                    StatusCode = 400
                };
            }
            // Validate required contact name
            if(string.IsNullOrEmpty(supplier.ContactName))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Provide a supplier  contact Name",
                    StatusCode = 400
                };
            }
            // Validate basic supplier email format
            if(!supplier.EmailContact.Contains("@") || !supplier.EmailContact.Contains("."))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Provide a correct supplier email",
                    StatusCode = 400
                };
            }
            // Validate supplier phone length and required value
            if(string.IsNullOrEmpty(supplier.PhoneContact) || supplier.PhoneContact.Length < 4 || supplier.PhoneContact.Length > 20)
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Provide a correct supplier phone",
                    StatusCode = 400
                };
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
                Created = DateOnly.FromDateTime(DateTime.UtcNow),
                LastUpdated = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            try
            {
                // Save the supplier to the repository
                var createdSupplier = await _supplierRepository.CreateSupplierAsync(newSupplier);
                // Build the response DTO from the created supplier
                var supplierResponse = new SupplierResponseDTO
                {
                    ID = createdSupplier.ID,
                    Name = createdSupplier.Name,
                    ContactName = createdSupplier.ContactName,
                    PhoneContact = createdSupplier.PhoneContact,
                    EmailContact = createdSupplier.EmailContact,
                };
                // Return the created supplier response
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = true,
                    Data = supplierResponse,
                    Message = "Supplier Successfully Created",
                    StatusCode = 201
                };
            }
            // Handle any exceptions that may occur while creating the supplier
            catch
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to create supplier.",
                    StatusCode = 500
                };
            }
        }


        // === PUT === \\
        public async Task<ApiResponse<SupplierResponseDTO>> UpdateSupplierAsync(int supplierId, UpdateSupplierRequestDTO updatedSupplier)
        {
            try
            {
                // Retrieve the supplier before applying updates
                var supplier = await _supplierRepository.GetSupplierByIdAsync(supplierId);
                if(supplier == null)
                {
                    return new ApiResponse<SupplierResponseDTO>
                    {
                        Success = false,
                        Message = "Supplier not found",
                        StatusCode = 404
                    };
                }
                
                // Apply the update DTO values to the supplier entity
                supplier.Name = updatedSupplier.Name;
                supplier.ContactName = updatedSupplier.ContactName;
                supplier.PhoneContact = updatedSupplier.PhoneContact;
                supplier.EmailContact = updatedSupplier.EmailContact;
                supplier.IsActive = updatedSupplier.IsActive;
                supplier.LastUpdated = DateOnly.FromDateTime(DateTime.UtcNow);

                // Save the updated supplier through the repository
                await _supplierRepository.UpdateSupplierAsync(supplier);

                // Build the response DTO from the updated supplier
                var supplierResponse = new SupplierResponseDTO
                {
                    Name = supplier.Name,
                    ContactName = supplier.ContactName,
                    PhoneContact = supplier.PhoneContact,
                    EmailContact = supplier.EmailContact,
                    IsActive = supplier.IsActive,
                };

                // Return the updated supplier details
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = true,
                    Data = supplierResponse,
                    Message = "Supplier details Successfully changed",
                    StatusCode = 200
                };
            }
            // Handle any exceptions that may occur while updating the supplier
            catch
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to update supplier.",
                    StatusCode = 500
                };
            }
        }

        // === SET ACTIVE STATUS === \\
        public async Task<ApiResponse<SupplierResponseDTO>> ActivateSupplierAsync(int supplierId)
        {
            try
            {
                // Retrieve the supplier before attempting to activate it
                var supplier = await _supplierRepository.GetSupplierByIdAsync(supplierId);
                if(supplier == null)
                {
                    return new ApiResponse<SupplierResponseDTO>
                    {
                        Success = false,
                        Message = "Supplier not found",
                        StatusCode = 404
                    };
                }
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
                supplier.LastUpdated = DateOnly.FromDateTime(DateTime.UtcNow);
                await _supplierRepository.SaveChangesAsync();

                // Return the activated supplier details
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = true,
                    Message = "Supplier successfully activated",
                    StatusCode = 200,
                    Data = new SupplierResponseDTO
                    {
                        ID = supplier.ID,
                        Name = supplier.Name,
                        ContactName = supplier.ContactName,
                        PhoneContact = supplier.PhoneContact,
                        EmailContact = supplier.EmailContact,
                        IsActive = supplier.IsActive
                    }
                };
            }
            // Handle any exceptions that may occur while activating the supplier
            catch
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to activate supplier.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<SupplierResponseDTO>> DeactivateSupplierAsync(int supplierId)
        {
            try
            {
                // Retrieve the supplier before attempting to deactivate it
                var supplier = await _supplierRepository.GetSupplierByIdAsync(supplierId);
                if(supplier == null)
                {
                    return new ApiResponse<SupplierResponseDTO>
                    {
                        Success = false,
                        Message = "Supplier not found",
                        StatusCode = 404
                    };
                }
                // Return a bad request response if the supplier is already inactive
                if(supplier.IsActive == false)
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
                supplier.LastUpdated = DateOnly.FromDateTime(DateTime.UtcNow);
                await _supplierRepository.SaveChangesAsync();

                // Return the deactivated supplier details
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = true,
                    Message = "Supplier successfully deactivated",
                    StatusCode = 200,
                    Data = new SupplierResponseDTO
                    {
                        ID = supplier.ID,
                        Name = supplier.Name,
                        ContactName = supplier.ContactName,
                        PhoneContact = supplier.PhoneContact,
                        EmailContact = supplier.EmailContact,
                        IsActive = supplier.IsActive
                    }
                };
            }
            // Handle any exceptions that may occur while deactivating the supplier
            catch
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Internal error occurred, failed to deactivate supplier.",
                    StatusCode = 500
                };
            }
        }
    }
}
