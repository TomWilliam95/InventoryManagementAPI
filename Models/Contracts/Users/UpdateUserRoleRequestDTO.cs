using InventoryManagementAPI.Models.CoreModels.RolePermissions;
using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.UserDTO_s
{
    public class UpdateUserRoleRequestDTO
    {
        [Required(ErrorMessage = "User role is required.")]
        public required string NewRole { get; set; }

        public byte[] RowVersion { get; set; } = [];
    }
}
