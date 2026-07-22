using BCrypt.Net;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.Enums;
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

        // === GET === \\
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRoles role)
        {
            return await _context.Users.Where(u => u.Role == role).ToListAsync();
        }

        // === POST === \\
        public async Task<User> CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // === CHECK EXISTENCE === \\
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.ID == userId);
        }

        // === CHECK ACTIVE STATUS === \\
        public async Task<bool> IsUserActiveAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.IsActive == true;
        }

        // === SAVE CHANGES === \\
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        
    }
}
