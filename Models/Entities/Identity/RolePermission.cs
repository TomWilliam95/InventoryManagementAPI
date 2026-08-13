using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Models.CoreModels.RolePermissions
{
    [PrimaryKey(nameof(RoleID), nameof(PermissionID))]
    public class RolePermission
    {
        public int RoleID { get; set; }
        public Role Role { get; set; } = null!;

        public int PermissionID { get; set; }
        public Permission Permission { get; set; } = null!;

        public DateTime Created { get; set; }
    }
}
