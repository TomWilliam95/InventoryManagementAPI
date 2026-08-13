using InventoryManagementAPI.Models.CoreModels.RolePermissions;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.PermissionRepositories
{
    public class PermissionRepository: IPermissionRepository
    {
        private readonly InvManDBContext _context;
        public PermissionRepository(InvManDBContext context)
        {
            _context = context;
        }

        public async Task<Permission?> GetPermissionByIdAsync(int permissionId, CancellationToken cancellationToken = default)
        {
            return await _context.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID == permissionId, cancellationToken);
        }

        public async Task<IEnumerable<Permission>?> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Permissions
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddPermission(Permission permission, CancellationToken cancellationToken)
        {
            await _context.Permissions.AddAsync(permission, cancellationToken);
        }
        
        public async Task<bool> CheckPermissionExistsAsync(string permissionName, CancellationToken cancellationToken = default)
        {
            return await _context.Permissions
                .AsNoTracking()
                .AnyAsync(p => p.Name == permissionName, cancellationToken);
        }
    }
}
