using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.MovementDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.InvMovementRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryMovementController : ControllerBase
    {
        // === Dependencies === \\
        private readonly IInventoryMovementService _inventoryManagementService;
        public InventoryMovementController(IInventoryMovementService inventoryManagementService)
        {
            _inventoryManagementService = inventoryManagementService;
        }

        // === GET === \\
        [HttpGet("MovementHistory/{id}")]
        public async Task<ActionResult<InventoryMovementResponseDTO>> GetMovementById(int id)
        {
            var result = await _inventoryManagementService.GetMovementByIdAsync(id);
            return result.StatusCode switch
            {
                200 => Ok(result),
                404 => NotFound(result),
                500 => StatusCode(500, result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        [HttpGet("AllMovements")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<IEnumerable<BulkInventoryMovementResponseDTO>>> GetAllMovements()
        {
            var result = await _inventoryManagementService.GetAllMovementsAsync();
            return result.StatusCode switch
            {
                200 => Ok(result),
                404 => NotFound(result),
                500 => StatusCode(500, result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        [HttpGet("ProductMovementHistory/{productId}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<IEnumerable<BulkInventoryMovementResponseDTO>>> GetProductMovementHistory(int productId)
        {
            var result = await _inventoryManagementService.GetProductMovementHistoryAsync(productId);
            return result.StatusCode switch
            {
                200 => Ok(result),
                404 => NotFound(result),
                500 => StatusCode(500, result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        [HttpGet("UserMovementHistory/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<BulkInventoryMovementResponseDTO>>> GetUserMovementHistory(int userId)
        {
            var result = await _inventoryManagementService.GetMovementsByUserIdAsync(userId);
            return result.StatusCode switch
            {
                200 => Ok(result),
                404 => NotFound(result),
                500 => StatusCode(500, result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        [HttpGet("MovementHistoryByDateRange/{startDate}-{endDate}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<IEnumerable<BulkInventoryMovementResponseDTO>>> GetMovementHistoryByDateRange(DateTime startDate, DateTime endDate)
        {
            var result = await _inventoryManagementService.GetMovementsByDateRangeAsync(startDate, endDate);
            return result.StatusCode switch
            {
                200 => Ok(result),
                400 => BadRequest(result),
                404 => NotFound(result),
                500 => StatusCode(500, result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        [HttpGet("MovementHistoryByType/{movementType}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<IEnumerable<BulkInventoryMovementResponseDTO>>> GetMovementHistoryByType(MovementType movementType)
        {
            var result = await _inventoryManagementService.GetMovementsByMovementTypeAsync(movementType);
            return result.StatusCode switch
            {
                200 => Ok(result),
                400 => BadRequest(result),
                404 => NotFound(result),
                500 => StatusCode(500, result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        // === POST === \\
        [HttpPost("RecordMovement")]
        public async Task<ActionResult<InventoryMovementResponseDTO>> RecordMovement(CreateInventoryMovementRequestDTO dto)
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
                201 => CreatedAtAction(nameof(GetMovementById), new { id = result.Data.ID }, result.Data),
                400 => BadRequest(result),
                404 => NotFound(result),
                500 => StatusCode(500, result),
                _ => StatusCode(result.StatusCode, result)
            };
        }
    }
}
