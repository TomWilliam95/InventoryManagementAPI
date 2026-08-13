namespace InventoryManagementAPI.Models.DTO_s.SupplierProductDTO_s;

public class SupplierProductResponseDTO
{
    public int SupplierID { get; set; }
    public required string SupplierName { get; set; }
    public int ProductID { get; set; }
    public required string ProductSku { get; set; }
    public required string ProductName { get; set; }
    public string? SupplierSku { get; set; }
    public decimal UnitCost { get; set; }
    public int LeadTimeDays { get; set; }
    public int MinimumOrderQuantity { get; set; }
    public bool IsPreferred { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
