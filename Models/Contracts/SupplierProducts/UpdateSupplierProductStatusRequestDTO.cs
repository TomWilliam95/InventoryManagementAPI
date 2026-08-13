namespace InventoryManagementAPI.Models.DTO_s.SupplierProductDTO_s;

public class UpdateSupplierProductStatusRequestDTO
{
    public bool IsActive { get; set; }

    [Required]
    public byte[] RowVersion { get; set; } = [];
}
