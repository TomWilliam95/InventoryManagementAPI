using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.MovementDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.InvMovementRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryManagementController : ControllerBase
    {
        // === Dependencies === \\
        private readonly IInventoryMovementService _inventoryManagementService;
        public InventoryManagementController(IInventoryMovementService inventoryManagementService)
        {
            _inventoryManagementService = inventoryManagementService;
        }

        // === POST ENDPOINTS === \\
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
                        StatusCode = 400,
                        Message = "Invalid movement type.",
                        Data = null
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


        // === GET ENDPOINTS === \\
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
    }
}
