using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.CoreModels.RolePermissions;
using InventoryManagementAPI.Models.DTO_s.UserDTO_s;

namespace InventoryManagementAPI.Repositories.UserRepositories
{
    public interface IUserService
    {
        // === GET ===
        Task<ApiResponse<IEnumerable<UserResponseDTO>>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<UserResponseDTO>> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserResponseDTO>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<UserResponseDTO>>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default);

        // === POST ===
        Task<ApiResponse<UserResponseDTO>> CreateUserAsync(CreateNewUserRequestDTO user, CancellationToken cancellationToken = default);

        // === PATCH ===
        Task<ApiResponse<UserResponseDTO>> UpdateUserNameAsync(int userId, UpdateUserNameRequestDTO nameRequest, int currentUserId, string currentUserRole, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserResponseDTO>> UpdateUserEmailAsync(int userId, UpdateUserEmailRequestDTO emailRequest, int currentUserId, string currentUserRole, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserResponseDTO>> UpdateUserPasswordAsync(int userId, UpdateUserPasswordRequestDTO passwordRequest, int currentUserId, string currentUserRole, CancellationToken cancellationToken = default);

        Task<ApiResponse<UserResponseDTO>> AssignUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserResponseDTO>> RemoveUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);

        // === SET ACTIVE / INACTIVE ===
        Task<ApiResponse<UserResponseDTO>> ActivateUserAsync(int userId, UpdateUserStatusRequestDTO statusRequest, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserResponseDTO>> DeactivateUserAsync(int userId, UpdateUserStatusRequestDTO statusRequest, CancellationToken cancellationToken = default);
    }
}
