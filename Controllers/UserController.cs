using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.UserRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // === Get === \\
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDTO>> GetSingleUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return user.StatusCode switch
            {
                200 => Ok(user),
                404 => NotFound(user),
                500 => StatusCode(500, user),
                _ => StatusCode(user.StatusCode, user)
            };
        }

        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<UserResponseDTO>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return users.StatusCode switch
            {
                200 => Ok(users),
                404 => NotFound(users),
                400 => BadRequest(users),
                500 => StatusCode(500, users),
                _ => StatusCode(users.StatusCode, users)
            };
        }

        [HttpGet("GetUserByEmail/{email}")]
        public async Task<ActionResult<UserResponseDTO>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            return user.StatusCode switch
            {
                200 => Ok(user),
                404 => NotFound(user),
                500 => StatusCode(500, user),
                _ => StatusCode(user.StatusCode, user)
            };
        }

        [HttpGet("GetUsersByRole/{role}")]
        public async Task<ActionResult<IEnumerable<UserResponseDTO>>> GetUsersByRole(UserRoles role)
        {
            var users = await _userService.GetUsersByRoleAsync(role);
            return users.StatusCode switch
            {
                200 => Ok(users),
                404 => NotFound(users),
                400 => BadRequest(users),
                500 => StatusCode(500, users),
                _ => StatusCode(users.StatusCode, users)
            };
        }

        // === Post === \\
        [HttpPost("CreateNewUser")]
        public async Task<ActionResult<UserResponseDTO>> CreateNewUser(CreateNewUserRequestDTO newUser)
        {
            var user = await _userService.CreateUserAsync(newUser);
            return user.StatusCode switch
            {
                201 => CreatedAtAction(nameof(GetSingleUser), new { id = user.Data.ID }, user),
                400 => BadRequest(user),

                500 => StatusCode(500, user),
                _ => StatusCode(user.StatusCode, user)
            };
        }

        // === Put === \\
        [HttpPut("UpdateUser/{userId}")]
        public async Task<ActionResult<UserResponseDTO>> UpdateUser(int userId, UpdateUserRequestDTO updatedUser)
        {
            var user = await _userService.UpdateUserAsync(userId, updatedUser);
            return user.StatusCode switch
            {
                200 => Ok(user),
                400 => BadRequest(user),
                404 => NotFound(user),
                500 => StatusCode(500, user),
                _ => StatusCode(user.StatusCode, user)
            };
        }

        // === Patch === \\
        [HttpPatch("UpdateUserRole/{userId}")]
        public async Task<ActionResult<UserResponseDTO>> UpdateUserRole(int userId, UpdateUserRoleRequestDTO newRole)
        {
            var user = await _userService.UpdateUserRoleAsync(userId, newRole);
            return user.StatusCode switch
            {
                200 => Ok(user),
                400 => BadRequest(user),
                404 => NotFound(user),
                500 => StatusCode(500, user),
                _ => StatusCode(user.StatusCode, user)
            };
        }

        // === Delete === \\


    }
}
