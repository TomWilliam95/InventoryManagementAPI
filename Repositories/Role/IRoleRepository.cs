using InventoryManagementAPI.Models.CoreModels.RolePermissions;

namespace InventoryManagementAPI.Repositories.UserRoles
{
    public interface IRoleRepository
    {
        Task<Role?> GetRoleAsync(int roleId, CancellationToken cancellationToken = default);
        Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>?> GetAllRolesAsync(CancellationToken cancellationToken = default);
        Task AddRoleAsync(Role role, CancellationToken cancellationToken = default);
        Task<bool> CheckRoleExistAsync(string roleName, CancellationToken cancellationToken = default);
    }
}
