using InventoryManagementAPI.Models.CoreModels.RolePermissions;

namespace InventoryManagementAPI.Repositories.UserRoleRepositories
{
    public interface IUserRoleRepository
    {
        Task<IReadOnlyList<Role>> GetUserRolesByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<User>> GetUsersByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);

        Task AssignUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);
        Task<bool> RemoveUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);
        Task<bool> UserRoleExistsAsync(int userId, int roleId, CancellationToken cancellationToken = default);
    }
}
