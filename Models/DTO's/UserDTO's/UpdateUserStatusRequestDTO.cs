namespace InventoryManagementAPI.Models.DTO_s.UserDTO_s
{
    public class UpdateUserStatusRequestDTO
    {
        [Required(ErrorMessage = "Active status is required.")]
        public bool IsActive { get; set; }

        public byte[] RowVersion { get; set; } = [];
    }
}
