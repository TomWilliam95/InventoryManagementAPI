namespace InventoryManagementAPI.Models.DTO_s.SupplierContactDTO_s;

public class CreateSupplierContactRequestDTO
{
    [Range(1, int.MaxValue)]
    public int? SupplierAddressID { get; set; }

    [Required, StringLength(150)]
    public required string Name { get; set; }

    [StringLength(100)]
    public string? JobTitle { get; set; }

    [EmailAddress, StringLength(254)]
    public string? Email { get; set; }

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    public bool IsPrimary { get; set; }
}
