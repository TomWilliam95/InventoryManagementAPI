using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.UserRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryManagementAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // === GET === \\
        [HttpGet("User/{id}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<UserResponseDTO>>> GetSingleUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return StatusCode(user.StatusCode, user);
        }

        [HttpGet("AllUsers")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<IEnumerable<UserResponseDTO>>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return StatusCode(users.StatusCode, users);
        }

        [HttpGet("Email/{email}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<UserResponseDTO>>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            return StatusCode(user.StatusCode, user);
        }

        [HttpGet("Role/{role}")]
        [Authorize(Roles = ("Admin"))]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<IEnumerable<UserResponseDTO>>>> GetUsersByRole(UserRoles role)
        {
            var users = await _userService.GetUsersByRoleAsync(role);
            return StatusCode(users.StatusCode, users);
        }


        // === POST === \\
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<UserResponseDTO>>> CreateNewUser(CreateNewUserRequestDTO newUser)
        {
            var user = await _userService.CreateUserAsync(newUser);
            return user.StatusCode switch
            {
                201 when user.Data is not null => CreatedAtAction(nameof(GetSingleUser), new { id = user.Data.ID }, user),
                _ => StatusCode(user.StatusCode, user)
            };
        }

        // === PATCH === \\
        [HttpPatch("UpdateRole/{userId}")]
        [Authorize(Roles = ("Admin"))]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<UserResponseDTO>>> UpdateUserRole(int userId, UpdateUserRoleRequestDTO newRole)
        {
            var user = await _userService.UpdateUserRoleAsync(userId, newRole);
            return StatusCode(user.StatusCode, user);
        }

        [HttpPatch("Password/{userId}")]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<UserResponseDTO>>> UpdateUserPassword(int userId, UpdateUserPasswordRequestDTO newPassword)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var admin = User.FindFirstValue(ClaimTypes.Role)!;

            var user = await _userService.UpdateUserPasswordAsync(userId, newPassword, currentUserId, admin);
            return StatusCode(user.StatusCode, user);
        }

        [HttpPatch("Email/{userId}")]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<UserResponseDTO>>> UpdateUserEmail(int userId, UpdateUserEmailRequestDTO newEmail)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) ;
            var admin = User.FindFirstValue(ClaimTypes.Role)!;

            var user = await _userService.UpdateUserEmailAsync(userId, newEmail, currentUserId, admin);
            return StatusCode(user.StatusCode, user);
        }

        [HttpPatch("Username/{userId}")]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<UserResponseDTO>>> UpdateUserUsername(int userId, UpdateUserNameRequestDTO newUsername)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var admin = User.FindFirstValue(ClaimTypes.Role)!;

            var user = await _userService.UpdateUserNameAsync(userId, newUsername, currentUserId, admin);
            return StatusCode(user.StatusCode, user);
        }

        // === SET ACTIVE / INACTIVE === \\
        [HttpPatch("Activate/{userId}")]
        [Authorize(Roles = ("Admin"))]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<UserResponseDTO>>> ActivateUser(int userId)
        {
            var user = await _userService.ActivateUserAsync(userId);
            return StatusCode(user.StatusCode, user);
        }

        [HttpPatch("Deactivate/{userId}")]
        [Authorize(Roles = ("Admin"))]
        public async Task<ActionResult<InventoryManagementAPI.Models.CoreModels.ApiResponse<UserResponseDTO>>> DeactivateUser(int userId)
        {
            var user = await _userService.DeactivateUserAsync(userId);
            return StatusCode(user.StatusCode, user);
        }
    }
}
