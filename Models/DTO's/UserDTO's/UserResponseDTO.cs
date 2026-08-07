using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.UserDTO_s
{
    public class UserResponseDTO
    {
        public int ID { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public UserRoles Role { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsActive { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
