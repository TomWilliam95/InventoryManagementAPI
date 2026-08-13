using InventoryManagementAPI.Models.CoreModels.RolePermissions;

namespace InventoryManagementAPI.Repositories.RolePermissionRepositories
{
    public interface IRolePermissionRepository
    {
        Task<IEnumerable<Permission>?> GetPermissionsByRoleID(int roleID, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>?> GetRolesByPermissionID(int permissionID, CancellationToken cancellationToken = default);
        Task AssignPermissionToRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken = default);
        Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken = default);
        Task<bool> CheckRolePermissionExistAsync(Role role, Permission permission, CancellationToken cancellationToken = default);
    }
}
