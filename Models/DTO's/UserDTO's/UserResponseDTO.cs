using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.UserDTO_s
{
    public class UserResponseDTO
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public UserRoles Role { get; set; }
        public DateOnly Created { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsActive { get; set; }
    }
}
