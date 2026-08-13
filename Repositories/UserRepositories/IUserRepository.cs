using InventoryManagementAPI.Models.CoreModels.RolePermissions;

namespace InventoryManagementAPI.Repositories.UserRepositories
{
    public interface IUserRepository
    {
        // === GET ===
        Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);

        Task<User?> GetUserWithRolesForAuthentication(string email, CancellationToken cancellationToken = default);

        // === POST ===
        Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default);

        // === CHECK EXISTENCE ===
        Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

        // === CHECK ACTIVE STATUS ===
        Task<bool> IsUserActiveAsync(int userId, CancellationToken cancellationToken = default);

        // === SAVE CHANGES ===
    }
}
