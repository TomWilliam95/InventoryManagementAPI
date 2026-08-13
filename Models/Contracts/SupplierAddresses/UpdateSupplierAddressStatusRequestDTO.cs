namespace InventoryManagementAPI.Models.DTO_s.SupplierAddressDTO_s;

public class UpdateSupplierAddressStatusRequestDTO
{
    public bool IsActive { get; set; }

    [Required]
    public byte[] RowVersion { get; set; } = [];
}
