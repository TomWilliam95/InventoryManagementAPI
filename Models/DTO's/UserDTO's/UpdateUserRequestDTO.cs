using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.UserDTO_s
{
    public class UpdateUserRequestDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
