namespace InventoryManagementAPI.Models.DTO_s.SupplierProductDTO_s;

public class UpdateSupplierProductPreferredRequestDTO
{
    public bool IsPreferred { get; set; }

    [Required]
    public byte[] RowVersion { get; set; } = [];
}
