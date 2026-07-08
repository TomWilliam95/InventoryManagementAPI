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
                var supplierList = await _supplierRepository.GetAllSuppliersAsync();

                if (supplierList == null || !supplierList.Any())
                {
                    return new ApiResponse<IEnumerable<SupplierResponseDTO>>
                    {
                        Success = false,
                        Message = "No Suppliers Found",
                        StatusCode = 404
                    };
                }
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
                return new ApiResponse<IEnumerable<SupplierResponseDTO>>
                {
                    Success = true,
                    Data = supplierDtoList,
                    Message = "Suppliers Succesffuly retrieved",
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<IEnumerable<SupplierResponseDTO>>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }
        public async Task<ApiResponse<SupplierResponseDTO>> GetSupplierByIdAsync(int supplierId)
        {
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
                    Message = "Supplier Succesffuly Retrieved",
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }


        // === POST === \\
        public async Task<ApiResponse<SupplierResponseDTO>> CreateSupplierAsync(CreateSupplierRequestDTO supplier)
        {
            if(supplier == null)
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Supplier Not Created",
                    StatusCode = 400
                };
            }
            if (String.IsNullOrEmpty(supplier.Name))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Provide a supplier Name",
                    StatusCode = 400
                };
            }
            if(string.IsNullOrEmpty(supplier.ContactName))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Provide a supplier  contact Name",
                    StatusCode = 400
                };
            }
            if(!supplier.EmailContact.Contains("@") || !supplier.EmailContact.Contains("."))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Provide a correct supplier email",
                    StatusCode = 400
                };
            }
            if(supplier.PhoneContact.Length > 5 || supplier.PhoneContact.Length > 20 || string.IsNullOrEmpty(supplier.PhoneContact))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Provide a correct supplier phone",
                    StatusCode = 400
                };
            }

            var newSupplier = new Supplier
            {
                Name = supplier.Name,
                ContactName = supplier.ContactName,
                PhoneContact = supplier.PhoneContact,
                EmailContact = supplier.EmailContact,
                IsActive = supplier.IsActive,
                Created = DateOnly.FromDateTime(DateTime.UtcNow),
                LastUpdated = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            try
            {
                var createdSupplier = await _supplierRepository.CreateSupplierAsync(newSupplier);
                var supplierResponse = new SupplierResponseDTO
                {
                    ID = createdSupplier.ID,
                    Name = createdSupplier.Name,
                    ContactName = createdSupplier.ContactName,
                    PhoneContact = createdSupplier.PhoneContact,
                    EmailContact = createdSupplier.EmailContact,
                };
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = true,
                    Data = supplierResponse,
                    Message = "Supplier Successfully Created",
                    StatusCode = 201
                };
            }
            catch
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }


        // === PUT === \\
        public async Task<ApiResponse<SupplierResponseDTO>> UpdateSupplierAsync(int supplierId, UpdateSupplierRequestDTO updatedSupplier)
        {
            try
            {
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
                
                supplier.Name = updatedSupplier.Name;
                supplier.ContactName = updatedSupplier.ContactName;
                supplier.PhoneContact = updatedSupplier.PhoneContact;
                supplier.EmailContact = updatedSupplier.EmailContact;
                supplier.IsActive = updatedSupplier.IsActive;
                supplier.LastUpdated = DateOnly.FromDateTime(DateTime.UtcNow);

                await _supplierRepository.UpdateSupplierAsync(supplier);

                var supplierResponse = new SupplierResponseDTO
                {
                    Name = supplier.Name,
                    ContactName = supplier.ContactName,
                    PhoneContact = supplier.PhoneContact,
                    EmailContact = supplier.EmailContact,
                    IsActive = supplier.IsActive,
                };

                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = true,
                    Data = supplierResponse,
                    Message = "Supplier details Successfully changed",
                    StatusCode = 200
                };
            }
            catch
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }

        // === SET ACTIVE STATUS === \\
        public async Task<ApiResponse<SupplierResponseDTO>> ActivateSupplierAsync(int supplierId)
        {
            try
            {
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
                if (supplier.IsActive)
                {
                    return new ApiResponse<SupplierResponseDTO>
                    {
                        Success = false,
                        Message = "Supplier is already active",
                        StatusCode = 400
                    };
                }

                supplier.IsActive = true;
                supplier.LastUpdated = DateOnly.FromDateTime(DateTime.UtcNow);
                await _supplierRepository.SaveChangesAsync();

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
            catch
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<SupplierResponseDTO>> DeactivateSupplierAsync(int supplierId)
        {
            try
            {
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
                if(supplier.IsActive == false)
                {
                    return new ApiResponse<SupplierResponseDTO>
                    {
                        Success = false,
                        Message = "Supplier is already inactive",
                        StatusCode = 400
                    };
                }

                supplier.IsActive = false;
                supplier.LastUpdated = DateOnly.FromDateTime(DateTime.UtcNow);
                await _supplierRepository.SaveChangesAsync();

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
            catch
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }
    }
}
