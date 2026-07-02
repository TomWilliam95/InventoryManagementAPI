using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.UserRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
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

        [HttpGet("email/{email}")]
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

        [HttpGet("role/{role}")]
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


        // === POST === \\
        [HttpPost]
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

        // === PATCH === \\
        [HttpPatch("{userId}/role")]
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

        [HttpPatch("{userId}/passwordReset")]
        public async Task<ActionResult<UserResponseDTO>> UpdateUserPassword(int userId, UpdateUserPasswordRequestDTO newPassword)
        {
            var user = await _userService.UpdateUserPasswordAsync(userId, newPassword);
            return user.StatusCode switch
            {
                200 => Ok(user),
                400 => BadRequest(user),
                404 => NotFound(user),
                500 => StatusCode(500, user),
                _ => StatusCode(user.StatusCode, user)
            };
        }

        [HttpPatch("{userId}/email")]
        public async Task<ActionResult<UserResponseDTO>> UpdateUserEmail(int userId, UpdateUserEmailRequestDTO newEmail)
        {
            var user = await _userService.UpdateUserEmailAsync(userId, newEmail);
            return user.StatusCode switch
            {
                200 => Ok(user),
                400 => BadRequest(user),
                404 => NotFound(user),
                500 => StatusCode(500, user),
                _ => StatusCode(user.StatusCode, user)
            };
        }

        [HttpPatch("{userId}/username")]
        public async Task<ActionResult<UserResponseDTO>> UpdateUserUsername(int userId, UpdateUserNameRequestDTO newUsername)
        {
            var user = await _userService.UpdateUserNameAsync(userId, newUsername);
            return user.StatusCode switch
            {
                200 => Ok(user),
                400 => BadRequest(user),
                404 => NotFound(user),
                500 => StatusCode(500, user),
                _ => StatusCode(user.StatusCode, user)
            };
        }

        // === DELETE === \\

        [HttpDelete("{userId}")]
        public async Task<ActionResult<object>> DeleteUser(int userId)
        {
            var result = await _userService.DeleteUserAsync(userId);
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
