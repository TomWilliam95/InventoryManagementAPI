namespace InventoryManagementAPI.Models.DTO_s.UserDTO_s
{
    public class UpdateUserPasswordRequestDTO
    {
        [Required(ErrorMessage = "Current password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Current password must be between 8 and 100 characters.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be between 8 and 100 characters.")]
 [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).+$", ErrorMessage = "New password must include uppercase, lowercase, digit, and special character.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Retype password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be between 8 and 100 characters.")]
 [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).+$", ErrorMessage = "New password must include uppercase, lowercase, digit, and special character.")]
        public string RetypePassword { get; set; } = string.Empty;

        public byte[] RowVersion { get; set; } = [];
    }
}
