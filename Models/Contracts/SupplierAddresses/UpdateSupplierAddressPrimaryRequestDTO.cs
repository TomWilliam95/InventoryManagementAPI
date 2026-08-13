namespace InventoryManagementAPI.Models.DTO_s.SupplierAddressDTO_s;

public class UpdateSupplierAddressPrimaryRequestDTO
{
    public bool IsPrimary { get; set; }

    [Required]
    public byte[] RowVersion { get; set; } = [];
}
