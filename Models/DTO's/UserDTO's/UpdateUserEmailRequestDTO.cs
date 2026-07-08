namespace InventoryManagementAPI.Models.DTO_s.UserDTO_s
{
    public class UpdateUserEmailRequestDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        [StringLength(254, ErrorMessage = "Email cannot be longer than 254 characters.")]
        public required string Email { get; set; }
    }
}
