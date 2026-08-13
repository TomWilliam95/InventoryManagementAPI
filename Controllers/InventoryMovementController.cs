using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.MovementDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.InvMovementRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public async Task<ActionResult<ApiResponse<InventoryMovementResponseDTO>>> GetMovementById(int id, CancellationToken cancellationToken = default)
        {
            var result = await _inventoryManagementService.GetMovementByIdAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>>> GetAllMovements(CancellationToken cancellationToken = default)
        {
            var result = await _inventoryManagementService.GetAllMovementsAsync(cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("~/api/products/{productId:int}/inventory-movements")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>>> GetProductMovementHistory(int productId, CancellationToken cancellationToken = default)
        {
            var result = await _inventoryManagementService.GetProductMovementHistoryAsync(productId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("~/api/users/{userId:int}/inventory-movements")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>>> GetUserMovementHistory(int userId, CancellationToken cancellationToken = default)
        {
            var result = await _inventoryManagementService.GetMovementsByUserIdAsync(userId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("date-range")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>>> GetMovementHistoryByDateRange(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var result = await _inventoryManagementService.GetMovementsByDateRangeAsync(startDate, endDate, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("types/{movementType}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InventoryMovementResponseDTO>>>> GetMovementHistoryByType(MovementType movementType, CancellationToken cancellationToken = default)
        {
            var result = await _inventoryManagementService.GetMovementsByMovementTypeAsync(movementType, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        // === POST ===
        [HttpPost]
        public async Task<ActionResult<ApiResponse<InventoryMovementResponseDTO>>> RecordMovement(CreateInventoryMovementRequestDTO dto, CancellationToken cancellationToken = default)
        {
            // Validate the user ID from the claims
            // This assumes that the user ID is stored in the claims as a string. Adjust the claim type as necessary based on your authentication setup.
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new ApiResponse<InventoryMovementResponseDTO>
                {
                    Success = false,
                    StatusCode = 401,
                    Message = "User ID claim is missing or invalid.",
                });
            }

            ApiResponse<InventoryMovementResponseDTO> result = new ApiResponse<InventoryMovementResponseDTO>();
            switch (dto.Movement)
            {
                case MovementType.StockIn:
                case MovementType.Purchase:
                    result = await _inventoryManagementService.RecordStockInAsync(dto, userId, cancellationToken);
                    break;
                case MovementType.StockOut:
                case MovementType.Sale:
                    result = await _inventoryManagementService.RecordStockOutAsync(dto, userId, cancellationToken);
                    break;
                case MovementType.AdjustmentIncrease:
                case MovementType.AdjustmentDecrease:
                    result = await _inventoryManagementService.RecordAdjustmentAsync(dto, userId, cancellationToken);
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
