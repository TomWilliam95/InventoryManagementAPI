namespace InventoryManagementAPI.Models.DTO_s.SupplierContactDTO_s;

public class UpdateSupplierContactPrimaryRequestDTO
{
    public bool IsPrimary { get; set; }

    [Required]
    public byte[] RowVersion { get; set; } = [];
}
