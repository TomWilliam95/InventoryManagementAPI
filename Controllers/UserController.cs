using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.CoreModels.RolePermissions;
using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Repositories.UserRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryManagementAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // === GET ===
        [HttpGet("{id:int}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> GetSingleUser(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetUserByIdAsync(id, cancellationToken);
            return StatusCode(user.StatusCode, user);
        }

        [HttpGet]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDTO>>>> GetAllUsers(CancellationToken cancellationToken = default)
        {
            var users = await _userService.GetAllUsersAsync(cancellationToken);
            return StatusCode(users.StatusCode, users);
        }

        [HttpGet("by-email/{email}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> GetUserByEmail(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetUserByEmailAsync(email, cancellationToken);
            return StatusCode(user.StatusCode, user);
        }

        [HttpGet("roles/{roleName}")]
        [Authorize(Roles = ("Admin"))]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDTO>>>> GetUsersByRole(string roleName, CancellationToken cancellationToken = default)
        {
            var users = await _userService.GetUsersByRoleAsync(roleName, cancellationToken);
            return StatusCode(users.StatusCode, users);
        }


        // === POST ===
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> CreateNewUser(CreateNewUserRequestDTO newUser, CancellationToken cancellationToken = default)
        {
            var user = await _userService.CreateUserAsync(newUser, cancellationToken);
            return user.StatusCode switch
            {
                201 when user.Data is not null => CreatedAtAction(nameof(GetSingleUser), new { id = user.Data.ID }, user),
                _ => StatusCode(user.StatusCode, user)
            };
        }

        // === USER ROLES ===
        [HttpPut("{userId:int}/roles/{roleId:int}")]
        [Authorize(Roles = "Admin")]
 public async Task<ActionResult<ApiResponse<UserResponseDTO>>> AssignUserRole(int userId, int roleId, CancellationToken cancellationToken)
        {
            var result = await _userService.AssignUserRoleAsync(userId, roleId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{userId:int}/roles/{roleId:int}")]
        [Authorize(Roles = "Admin")]
 public async Task<ActionResult<ApiResponse<UserResponseDTO>>> RemoveUserRole(int userId, int roleId, CancellationToken cancellationToken)
        {
            var result = await _userService.RemoveUserRoleAsync(userId, roleId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        // === PATCH ===

        [HttpPatch("{userId:int}/password")]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> UpdateUserPassword(int userId, UpdateUserPasswordRequestDTO newPassword, CancellationToken cancellationToken = default)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var admin = User.FindFirstValue(ClaimTypes.Role)!;

            var user = await _userService.UpdateUserPasswordAsync(userId, newPassword, currentUserId, admin, cancellationToken);
            return StatusCode(user.StatusCode, user);
        }

        [HttpPatch("{userId:int}/email")]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> UpdateUserEmail(int userId, UpdateUserEmailRequestDTO newEmail, CancellationToken cancellationToken = default)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var admin = User.FindFirstValue(ClaimTypes.Role)!;

            var user = await _userService.UpdateUserEmailAsync(userId, newEmail, currentUserId, admin, cancellationToken);
            return StatusCode(user.StatusCode, user);
        }

        [HttpPatch("{userId:int}/username")]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> UpdateUserUsername(int userId, UpdateUserNameRequestDTO newUsername, CancellationToken cancellationToken = default)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var admin = User.FindFirstValue(ClaimTypes.Role)!;

            var user = await _userService.UpdateUserNameAsync(userId, newUsername, currentUserId, admin, cancellationToken);
            return StatusCode(user.StatusCode, user);
        }

        // === SET ACTIVE / INACTIVE ===
        [HttpPatch("{userId:int}/activate")]
        [Authorize(Roles = ("Admin"))]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> ActivateUser(int userId, UpdateUserStatusRequestDTO statusRequest, CancellationToken cancellationToken = default)
        {
            var user = await _userService.ActivateUserAsync(userId, statusRequest, cancellationToken);
            return StatusCode(user.StatusCode, user);
        }

        [HttpPatch("{userId:int}/deactivate")]
        [Authorize(Roles = ("Admin"))]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> DeactivateUser(int userId, UpdateUserStatusRequestDTO statusRequest, CancellationToken cancellationToken = default)
        {
            var user = await _userService.DeactivateUserAsync(userId, statusRequest, cancellationToken);
            return StatusCode(user.StatusCode, user);
        }
    }
}
