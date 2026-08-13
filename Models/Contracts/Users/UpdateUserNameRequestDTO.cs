namespace InventoryManagementAPI.Models.DTO_s.UserDTO_s
{
    public class UpdateUserNameRequestDTO
    {
        [Required(ErrorMessage = "User name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "User name must be between 3 and 100 characters.")]
        public required string UserName { get; set; }

        public byte[] RowVersion { get; set; } = [];
    }
}
