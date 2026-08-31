using InventoryManagementAPI.Models.CoreModels.RolePermissions;
using InventoryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories.UserRepositories
{
    public class UserRepository : IUserRepository
    {
        private readonly InvManDBContext _context;
        public UserRepository(InvManDBContext context)
        {
            _context = context;
        }

        // === GET ===
        public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }
        public async Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Users.FindAsync(userId, cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
 .Where(userRole => userRole.UserRoles.Any(userRole => userRole.Role.Name == roleName))
                .ToListAsync(cancellationToken);
        }


        public async Task<User?> GetUserWithRolesForAuthentication(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
        }


        // === POST ===\\
        public async Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            return user;
        }

        // === CHECK EXISTENCE ===
        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AsNoTracking().AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AsNoTracking().AnyAsync(u => u.ID == userId, cancellationToken);
        }

        // === CHECK ACTIVE STATUS ===
        public async Task<bool> IsUserActiveAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FindAsync(userId, cancellationToken);
            return user?.IsActive == true;
        }
    }
}
