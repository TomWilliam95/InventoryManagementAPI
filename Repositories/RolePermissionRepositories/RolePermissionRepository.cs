using InventoryManagementAPI.Models.CoreModels.RolePermissions;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.RolePermissionRepositories
{
    public class RolePermissionRepository: IRolePermissionRepository
    {
        private readonly InvManDBContext _context;
        public RolePermissionRepository(InvManDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Permission>?> GetPermissionsByRoleID(int roleID, CancellationToken cancellationToken = default)
        {
            return await _context.RolePermissions
                .AsNoTracking()
                .Where(rp => rp.RoleID == roleID)
                .Select(rp => rp.Permission)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Role>?> GetRolesByPermissionID(int permissionID, CancellationToken cancellationToken = default)
        {
            return await _context.RolePermissions
                .AsNoTracking()
                .Where(rp => rp.PermissionID == permissionID)
                .Select(rp => rp.Role)
                .ToListAsync(cancellationToken);
        }

 public async Task AssignPermissionToRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
        {
            await _context.RolePermissions.AddAsync(new RolePermission
            {
                RoleID = roleId,
                PermissionID = permissionId
            }, cancellationToken);
        }

 public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
        {
            var rolePermission = await _context.RolePermissions
 .SingleOrDefaultAsync(rp => rp.RoleID == roleId && rp.PermissionID == permissionId, cancellationToken);

            if (rolePermission is null)
                return false;

            _context.RolePermissions.Remove(rolePermission);
            return true;
        }


        public async Task<bool> CheckRolePermissionExistAsync(Role role, Permission permission, CancellationToken cancellationToken = default)
        {
            return await _context.RolePermissions
                .AsNoTracking()
                .AnyAsync(rp => rp.RoleID == role.ID && rp.PermissionID == permission.ID, cancellationToken);
        }
    }
}
