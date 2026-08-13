namespace InventoryManagementAPI.Models.DTO_s.SupplierProductDTO_s;

public class UpdateSupplierProductRequestDTO
{
    [StringLength(100)]
    public string? SupplierSku { get; set; }
    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal UnitCost { get; set; }
    [Range(0, 3650)]
    public int LeadTimeDays { get; set; }
    [Range(1, int.MaxValue)]
    public int MinimumOrderQuantity { get; set; } = 1;
    public bool IsPreferred { get; set; }

    [Required]
    public byte[] RowVersion { get; set; } = [];
}
