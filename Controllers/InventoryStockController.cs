using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.InventoryStockDTO_s;
using InventoryManagementAPI.Repositories.InventoryStockRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace InventoryManagementAPI.Controllers
{
    [Route("api/inventory-stocks")]
    [ApiController]
    [Authorize]
    public class InventoryStockController : ControllerBase
    {
        private readonly IInventoryStockService _inventoryStockService;
        public InventoryStockController(IInventoryStockService inventoryStockService)
        {
            _inventoryStockService = inventoryStockService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkInventoryStockResponseDTO>>>> GetAllStock(CancellationToken cancellationToken = default)
        {
            var result = await _inventoryStockService.GetAllInventoryStocksAsync(cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{productId:int}/{warehouseId:int}")]
        public async Task<ActionResult<ApiResponse<InventoryStockResponseDTO>>> GetStockByProductAndWarehouseId(int productId, int warehouseId, CancellationToken cancellationToken = default)
        {
            var result = await _inventoryStockService.GetInventoryStockByProductAndWarehouseIdAsync(productId, warehouseId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("warehouse/{warehouseId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkInventoryStockResponseDTO>>>> GetAllStockByWarehouse(int warehouseId, CancellationToken cancellationToken = default)
        {
            var result = await _inventoryStockService.GetInventoryStocksByWarehouseIdAsync(warehouseId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("product/{productId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkInventoryStockResponseDTO>>>> GetAllStockByProduct(int productId, CancellationToken cancellationToken = default)
        {
            var result = await _inventoryStockService.GetInventoryStocksByProductIdAsync(productId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("below-reorder-level")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkInventoryStockResponseDTO>>>> GetStockBelowReorderLevel(CancellationToken cancellationToken = default)
        {
            var result = await _inventoryStockService.GetInventoryStocksBelowReorderLevelAsync(cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{inventoryStockId:int}/reorder-level")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<InventoryStockResponseDTO>>> UpdateReorderLevel(int inventoryStockId,
            UpdateReorderLevelRequestDTO dto, CancellationToken cancellationToken = default)
        {
            var result = await _inventoryStockService.UpdateReorderLevelAsync(inventoryStockId, dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{inventoryStockId:int}/activate")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<InventoryStockResponseDTO>>> ActivateInventoryStock(int inventoryStockId,
            UpdateInventoryStockStatusRequestDTO dto, CancellationToken cancellationToken = default)
        {
            var result = await _inventoryStockService.ActivateInventoryStockAsync(inventoryStockId, dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{inventoryStockId:int}/deactivate")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<InventoryStockResponseDTO>>> DeactivateInventoryStock(int inventoryStockId,
            UpdateInventoryStockStatusRequestDTO dto, CancellationToken cancellationToken = default)
        {
            var result = await _inventoryStockService.DeactivateInventoryStockAsync(inventoryStockId, dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<InventoryStockResponseDTO>>> CreateInventoryStock(CreateInventoryStockRequestDTO dto, CancellationToken cancellationToken = default)
        {
            var result = await _inventoryStockService.CreateInventoryStockAsync(dto, cancellationToken);
            return result.StatusCode == StatusCodes.Status201Created && result.Data is not null
                ? CreatedAtAction(nameof(GetStockByProductAndWarehouseId),
                    new { productId = result.Data.ProductID, warehouseId = result.Data.WarehouseID }, result)
                : StatusCode(result.StatusCode, result);
        }
    }
}
