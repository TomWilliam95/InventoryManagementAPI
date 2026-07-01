using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.UserDTO_s
{
    public class UpdateUserRequestDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }
    }
}
