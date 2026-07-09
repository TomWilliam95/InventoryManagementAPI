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

        // === CHECK EXISTENCE === \\
        Task<bool> UserExistsAsync(int userId);
        Task<bool> EmailExistsAsync(string email);

        // === CHECK ACTIVE STATUS === \\
        Task<bool> IsUserActiveAsync(int userId);

        // === SAVE CHANGES === \\
        Task SaveChangesAsync();
    }
}
