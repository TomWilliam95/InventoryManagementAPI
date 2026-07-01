using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.UserDTO_s
{
    public class UpdateUserRoleRequestDTO
    {
        [EnumDataType(typeof(UserRoles))]
        public UserRoles NewRole { get; set; }
    }
}
