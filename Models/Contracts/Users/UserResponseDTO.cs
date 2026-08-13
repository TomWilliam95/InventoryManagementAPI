using InventoryManagementAPI.Models.CoreModels.RolePermissions;
using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.UserDTO_s
{
    public class UserResponseDTO
    {
        public int ID { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public Role Role { get; set; } = null!;
        public DateTime Created { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime Updated { get; set; }
        public bool IsActive { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
