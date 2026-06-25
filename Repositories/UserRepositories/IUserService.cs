using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Repositories.UserRepositories
{
    public interface IUserService
    {
        Task<ApiResponse<IEnumerable<UserResponseDTO>>> GetAllUsersAsync();
        Task<ApiResponse<UserResponseDTO>> GetUserByIdAsync(int userId);
        Task<ApiResponse<UserResponseDTO>> GetUserByEmailAsync(string email);
        Task<ApiResponse<IEnumerable<UserResponseDTO>>> GetUsersByRoleAsync(UserRoles role);

        Task<ApiResponse<UserResponseDTO>> AuthenticateUserAsync(LoginRequestDTO loginRequestDTO);

        Task<ApiResponse<UserResponseDTO>> CreateUserAsync(CreateNewUserRequestDTO user);

        Task<ApiResponse<UserResponseDTO>> UpdateUserAsync(int userId, UpdateUserRequestDTO updatedUser);
        Task<ApiResponse<UserResponseDTO>> UpdateUserRoleAsync(int userId, UpdateUserRoleRequestDTO roleRequest);

        Task<ApiResponse<object>> DeleteUserAsync(int userId);
    }
}
