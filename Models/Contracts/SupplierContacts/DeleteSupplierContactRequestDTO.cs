namespace InventoryManagementAPI.Models.DTO_s.SupplierContactDTO_s;

public class DeleteSupplierContactRequestDTO
{
    [Required]
    public byte[] RowVersion { get; set; } = [];
}
