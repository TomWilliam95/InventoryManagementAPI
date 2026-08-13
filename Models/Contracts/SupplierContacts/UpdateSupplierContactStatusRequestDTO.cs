namespace InventoryManagementAPI.Models.DTO_s.SupplierContactDTO_s;

public class UpdateSupplierContactStatusRequestDTO
{
    public bool IsActive { get; set; }
    [Required]
    public byte[] RowVersion { get; set; } = [];
}
