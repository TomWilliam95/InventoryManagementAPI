using InventoryManagementAPI.Models.DTO_s.SupplierDTO_s;
using InventoryManagementAPI.Repositories.SupplierRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementAPI.Controllers
{
    [Route("api/suppliers")]
    [ApiController]
    [Authorize]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        // === GET ===
        [HttpGet("{id:int}")]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<SupplierResponseDTO>>> GetSingleSupplier(int id)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
            return StatusCode(supplier.StatusCode, supplier);
        }

        [HttpGet]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<IEnumerable<SupplierResponseDTO>>>> GetAllSuppliers()
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            return StatusCode(suppliers.StatusCode, suppliers);
        }

        // === POST ===
        [HttpPost]
        [Authorize (Policy = ("AdminOrManager"))]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<SupplierResponseDTO>>> AddSupplier(CreateSupplierRequestDTO supplierDTO)
        {
            var createdSupplier = await _supplierService.CreateSupplierAsync(supplierDTO);
            return createdSupplier.StatusCode switch
            {
                201 when createdSupplier.Data is not null => CreatedAtAction(nameof(GetSingleSupplier), new { id = createdSupplier.Data.ID }, createdSupplier),
                _ => StatusCode(createdSupplier.StatusCode, createdSupplier)
            };
        }

        // === PUT ===
        [HttpPut("{supplierId:int}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<SupplierResponseDTO>>> EditSupplierDetails(int supplierId, UpdateSupplierRequestDTO supplierDTO)
        {
            var updatedSupplier = await _supplierService.UpdateSupplierAsync(supplierId, supplierDTO);
            return StatusCode(updatedSupplier.StatusCode, updatedSupplier);
        }

        // === SET ACTIVE STATUS ===
        [HttpPatch("{supplierId:int}/activate")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<SupplierResponseDTO>>> ActivateSupplier(int supplierId, UpdateSupplierStatusRequestDTO dto)
        {
            var activatedSupplier = await _supplierService.ActivateSupplierAsync(supplierId, dto);
            return StatusCode(activatedSupplier.StatusCode, activatedSupplier);
        }

        [HttpPatch("{supplierId:int}/deactivate")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<SupplierResponseDTO>>> DeactivateSupplier(int supplierId, UpdateSupplierStatusRequestDTO dto)
        {
            var deactivatedSupplier = await _supplierService.DeactivateSupplierAsync(supplierId, dto);
            return StatusCode(deactivatedSupplier.StatusCode, deactivatedSupplier);
        }
    }
}
