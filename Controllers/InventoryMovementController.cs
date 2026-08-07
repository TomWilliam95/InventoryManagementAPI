using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.MovementDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.InvMovementRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementAPI.Controllers
{
    [Route("api/inventory-movements")]
    [ApiController]
    [Authorize]
    public class InventoryMovementController : ControllerBase
    {
        // === Dependencies ===
        private readonly IInventoryMovementService _inventoryManagementService;
        public InventoryMovementController(IInventoryMovementService inventoryManagementService)
        {
            _inventoryManagementService = inventoryManagementService;
        }

        // === GET ===
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<InventoryMovementResponseDTO>>> GetMovementById(int id)
        {
            var result = await _inventoryManagementService.GetMovementByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>>> GetAllMovements()
        {
            var result = await _inventoryManagementService.GetAllMovementsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("~/api/products/{productId:int}/inventory-movements")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>>> GetProductMovementHistory(int productId)
        {
            var result = await _inventoryManagementService.GetProductMovementHistoryAsync(productId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("~/api/users/{userId:int}/inventory-movements")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>>> GetUserMovementHistory(int userId)
        {
            var result = await _inventoryManagementService.GetMovementsByUserIdAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("date-range")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>>> GetMovementHistoryByDateRange(DateTime startDate, DateTime endDate)
        {
            var result = await _inventoryManagementService.GetMovementsByDateRangeAsync(startDate, endDate);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("types/{movementType}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkInventoryMovementResponseDTO>>>> GetMovementHistoryByType(MovementType movementType)
        {
            var result = await _inventoryManagementService.GetMovementsByMovementTypeAsync(movementType);
            return StatusCode(result.StatusCode, result);
        }

        // === POST ===
        [HttpPost]
        public async Task<ActionResult<ApiResponse<InventoryMovementResponseDTO>>> RecordMovement(CreateInventoryMovementRequestDTO dto)
        {
            ApiResponse<InventoryMovementResponseDTO> result = new ApiResponse<InventoryMovementResponseDTO>();
            switch (dto.Movement)
            {
                case MovementType.StockIn:
                case MovementType.Purchase:
                    result = await _inventoryManagementService.RecordStockInAsync(dto);
                    break;
                case MovementType.StockOut:
                case MovementType.Sale:
                    result = await _inventoryManagementService.RecordStockOutAsync(dto);
                    break;
                case MovementType.AdjustmentIncrease:
                case MovementType.AdjustmentDecrease:
                    result = await _inventoryManagementService.RecordAdjustmentAsync(dto);
                    break;
                default:
                    return BadRequest(new ApiResponse<InventoryMovementResponseDTO>
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Invalid movement type.",
                    });
            }

            return result.StatusCode switch
            {
                201 when result.Data is not null  =>
                CreatedAtAction(nameof(GetMovementById), new { id = result.Data.ID }, result),
                _ => StatusCode(result.StatusCode, result)
            };
        }
    }
}
