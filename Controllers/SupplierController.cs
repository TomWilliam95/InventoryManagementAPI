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

        // === GET === \\
        [HttpGet("Supplier/{id}")]
        public async Task<ActionResult<SupplierResponseDTO>> GetSingleSupplier(int id)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
            return supplier.StatusCode switch
            {
                200 => Ok(supplier.Data),
                400 => BadRequest(supplier.Message),
                404 => NotFound(supplier.Message),
                500 => StatusCode(500, supplier.Message),
                _ => StatusCode(supplier.StatusCode, supplier)
            };
        }

        [HttpGet("AllSuppliers")]
        public async Task<ActionResult<IEnumerable<SupplierResponseDTO>>> GetAllSuppliers()
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            return suppliers.StatusCode switch
            {
                200 => Ok(suppliers),
                404 => NotFound(suppliers),
                500 => StatusCode(500, suppliers),
                _ => StatusCode(suppliers.StatusCode, suppliers)
            };
        }

        // === POST === \\
        [HttpPost("AddSupplier")]
        [Authorize (Policy = ("AdminOrManager"))]
        public async Task<ActionResult<SupplierResponseDTO>> AddSupplier(CreateSupplierRequestDTO supplierDTO)
        {
            var createdSupplier = await _supplierService.CreateSupplierAsync(supplierDTO);
            return createdSupplier.StatusCode switch
            {
                201 => CreatedAtAction(nameof(GetSingleSupplier), new { id = createdSupplier.Data.ID }, createdSupplier),
                400 => BadRequest(createdSupplier.Message),
                500 => StatusCode(500, createdSupplier.Message),
                _ => StatusCode(createdSupplier.StatusCode, createdSupplier)
            };
        }

        // === PUT === \\
        [HttpPut("EditSupplier/{supplierId}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<SupplierResponseDTO>> EditSupplierDetails(int supplierId, UpdateSupplierRequestDTO supplierDTO)
        {
            var updatedSupplier = await _supplierService.UpdateSupplierAsync(supplierId, supplierDTO);
            return updatedSupplier.StatusCode switch
            {
                200 => Ok(updatedSupplier.Data),
                400 => BadRequest(updatedSupplier.Message),
                404 => NotFound(updatedSupplier.Message),
                500 => StatusCode(500, updatedSupplier.Message),
                _ => StatusCode(updatedSupplier.StatusCode, updatedSupplier)
            };
        }

        // === DELETE === \\
        [HttpDelete("DeleteSupplier/{supplierId}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<SupplierResponseDTO>> DeleteSupplier(int supplierId)
        {
            var deletedSupplier = await _supplierService.DeleteSupplierAsync(supplierId);
            return deletedSupplier.StatusCode switch
            {
                204 => Ok(deletedSupplier.Data),
                404 => NotFound(deletedSupplier.Message),
                500 => StatusCode(500, deletedSupplier.Message),
                _ => StatusCode(500, deletedSupplier)
            };
        }
    }
}
