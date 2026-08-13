using InventoryManagementAPI.Models.CoreModels.UserModels;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Models.CoreModels.RolePermissions
{
    [PrimaryKey(nameof(UserID), nameof(RoleID))]
    public class UserRole
    {
        public int UserID { get; set; }
        public User User { get; set; } = null!;

        public int RoleID { get; set; }
        public Role Role { get; set; } = null!;

        public DateTime Created { get; set; }
    }
}
