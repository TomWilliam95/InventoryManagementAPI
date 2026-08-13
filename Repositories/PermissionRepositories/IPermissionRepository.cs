using InventoryManagementAPI.Models.CoreModels.RolePermissions;

namespace InventoryManagementAPI.Repositories.PermissionRepositories
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<Permission>?> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
        Task<Permission?> GetPermissionByIdAsync(int permissionId, CancellationToken cancellationToken = default);
        Task AddPermission(Permission permission, CancellationToken cancellationToken = default);
        Task<bool> CheckPermissionExistsAsync(string permissionName, CancellationToken cancellationToken = default);
    }
}
