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

        // === PATCH === \\
        public async Task<bool> UpdateUserEmailAsync(int userId, string email)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false; 
            
            user.Email = email;
            user.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserNameAsync(int userId, string userName)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.UserName = userName;
            user.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.Password_Hash = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword);
            user.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, UserRoles newRole)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            
            user.Role = newRole;
            user.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // === DELETE === \\
        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if(user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
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
    }
}
