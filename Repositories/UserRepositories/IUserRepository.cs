using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Repositories.UserRepositories
{
    public interface IUserRepository
    {
        // === GET === \\
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRoles role);
        Task<IEnumerable<User>> GetAllUsersAsync();

        // === POST === \\
        Task<User> CreateUserAsync(User user);

        // === PATCH === \\
        Task<bool> UpdateUserRoleAsync(int userId, UserRoles newRole);
        Task<bool> UpdateUserNameAsync(int userId, string userName);
        Task<bool> UpdateUserEmailAsync(int userId, string email);
        Task<bool> UpdateUserPasswordAsync(int userId, string newPassword);

        // === DELETE === \\
        Task<bool> DeleteUserAsync(int userId);

        // === CHECK EXISTENCE === \\
        Task<bool> UserExistsAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
    }
}
