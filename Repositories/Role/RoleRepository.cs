using InventoryManagementAPI.Models.CoreModels.RolePermissions;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.UserRoles
{
    public class RoleRepository : IRoleRepository
    {
        private readonly InvManDBContext _context;
        public RoleRepository(InvManDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>?> GetAllRolesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Role?> GetRoleAsync(int roleId, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ID == roleId, cancellationToken);
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
        }

        public async Task AddRoleAsync(Role role, CancellationToken cancellationToken = default)
        {
            await _context.Roles.AddAsync(role, cancellationToken); 
        }

        public async Task<bool> CheckRoleExistAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .AsNoTracking().
                AnyAsync(r => r.Name == roleName, cancellationToken);
        }
    }
}
