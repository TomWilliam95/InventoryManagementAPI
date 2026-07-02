using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.UserDTO_s;
using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Repositories.UserRepositories
{
    public interface IUserService
    {
        // === GET === \\
        Task<ApiResponse<IEnumerable<UserResponseDTO>>> GetAllUsersAsync();
        Task<ApiResponse<UserResponseDTO>> GetUserByIdAsync(int userId);
        Task<ApiResponse<UserResponseDTO>> GetUserByEmailAsync(string email);
        Task<ApiResponse<IEnumerable<UserResponseDTO>>> GetUsersByRoleAsync(UserRoles role);
        
        // === POST === \\
        Task<ApiResponse<UserResponseDTO>> CreateUserAsync(CreateNewUserRequestDTO user);

        // === PATCH === \\
        Task<ApiResponse<UserResponseDTO>> UpdateUserNameAsync(int userId, UpdateUserNameRequestDTO nameRequest);
        Task<ApiResponse<UserResponseDTO>> UpdateUserEmailAsync(int userId, UpdateUserEmailRequestDTO emailRequest);
        Task<ApiResponse<UserResponseDTO>> UpdateUserPasswordAsync(int userId, UpdateUserPasswordRequestDTO passwordRequest);
        Task<ApiResponse<UserResponseDTO>> UpdateUserRoleAsync(int userId, UpdateUserRoleRequestDTO roleRequest);

        // === DELETE === \\
        Task<ApiResponse<object>> DeleteUserAsync(int userId);

        // === AUTHENTICATION === \\
        Task<ApiResponse<UserResponseDTO>> AuthenticateUserAsync(LoginRequestDTO loginRequestDTO);
    }
}
