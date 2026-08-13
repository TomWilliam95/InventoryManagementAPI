using InventoryManagementAPI.Models.CoreModels.RolePermissions;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.UserRoleRepositories
{
    public class UserRoleRepository: IUserRoleRepository
    {
        private readonly InvManDBContext _context;
        public UserRoleRepository(InvManDBContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Role>> GetUserRolesByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserRoles.AsNoTracking()
                .Where(ur => ur.UserID == userId)
                .Select(ur => ur.Role)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<User>> GetUsersByRoleIdAsync(
            int roleId,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserRoles.AsNoTracking()
                .Where(ur => ur.RoleID == roleId)
                .Select(ur => ur.User)
                .ToListAsync(cancellationToken);
        }

        public async Task AssignUserRoleAsync(
            int userId,
            int roleId,
            CancellationToken cancellationToken = default)
        {
            await _context.UserRoles.AddAsync(new UserRole
            {
                UserID = userId,
                RoleID = roleId
            }, cancellationToken);
        }

        public async Task<bool> RemoveUserRoleAsync(
            int userId,
            int roleId,
            CancellationToken cancellationToken = default)
        {
            var userRole = await _context.UserRoles.SingleOrDefaultAsync(
                ur => ur.UserID == userId && ur.RoleID == roleId,
                cancellationToken);

            if (userRole is null)
                return false;

            _context.UserRoles.Remove(userRole);
            return true;
        }

        public async Task<bool> UserRoleExistsAsync(
            int userId,
            int roleId,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserRoles.AsNoTracking()
                .AnyAsync(
                    ur => ur.UserID == userId && ur.RoleID == roleId,
                    cancellationToken);
        }
    }
}
