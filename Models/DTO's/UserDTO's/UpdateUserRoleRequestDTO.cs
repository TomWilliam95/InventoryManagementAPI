using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.UserDTO_s
{
    public class UpdateUserRoleRequestDTO
    {
        [Required(ErrorMessage = "User role is required.")]
        [EnumDataType(typeof(UserRoles))]
        public UserRoles NewRole { get; set; }

        public byte[] RowVersion { get; set; } = [];
    }
}
