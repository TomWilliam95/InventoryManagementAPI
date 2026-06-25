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
            catch (Exception)
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
            if(! await _supplierRepository.SupplierExistsAsync(supplierId))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "No Supplier Found with this ID",
                    StatusCode = 404
                };
            }
            try
            {
                var supplier = await _supplierRepository.GetSupplierByIdAsync(supplierId);

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
            catch(Exception)
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
                    Message = "Supplier Not Created, Invalid Inputs",
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
            catch (Exception)
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
            if(! await _supplierRepository.SupplierExistsAsync(supplierId))
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Supplier not found",
                    StatusCode = 404
                };
            }
            try
            {
                var newUpdateSupplier = await _supplierRepository.GetSupplierByIdAsync(supplierId);
                newUpdateSupplier.Name = updatedSupplier.Name;
                newUpdateSupplier.ContactName = updatedSupplier.ContactName;
                newUpdateSupplier.PhoneContact = updatedSupplier.PhoneContact;
                newUpdateSupplier.EmailContact = updatedSupplier.EmailContact;
                newUpdateSupplier.IsActive = updatedSupplier.IsActive;
                newUpdateSupplier.LastUpdated = DateOnly.FromDateTime(DateTime.UtcNow);

                var supplierResponse = new SupplierResponseDTO
                {
                    Name = newUpdateSupplier.Name,
                    ContactName = newUpdateSupplier.ContactName,
                    PhoneContact = newUpdateSupplier.PhoneContact,
                    EmailContact = newUpdateSupplier.EmailContact,
                    IsActive = newUpdateSupplier.IsActive,
                };

                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = true,
                    Data = supplierResponse,
                    Message = "Supplier details Successfully changed",
                    StatusCode = 200
                };
            }
            catch (Exception)
            {
                return new ApiResponse<SupplierResponseDTO>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }


        // === DELETE === \\
        public async Task<ApiResponse<object>> DeleteSupplierAsync(int supplierId)
        {
            if(! await _supplierRepository.SupplierExistsAsync(supplierId))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "No existing Supplier with ID",
                    StatusCode = 404
                };
            }
            try
            {
                var supplierDeleteResult = await _supplierRepository.DeleteSupplierAsync(supplierId);

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "Supplier Deleted",
                    StatusCode = 204
                };
            }
            catch(Exception)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    StatusCode = 500
                };
            }
        }
    }
}
