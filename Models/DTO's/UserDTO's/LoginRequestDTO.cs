namespace InventoryManagementAPI.Models.DTO_s.UserDTO_s
{
    public class LoginRequestDTO
    {
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }
    }
}
