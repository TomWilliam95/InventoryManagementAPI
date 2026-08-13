namespace InventoryManagementAPI.Models.DTO_s.SupplierContactDTO_s;

public class SupplierContactResponseDTO
{
    public int ID { get; set; }
    public int SupplierID { get; set; }
    public int? SupplierAddressID { get; set; }
    public required string Name { get; set; }
    public string? JobTitle { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
