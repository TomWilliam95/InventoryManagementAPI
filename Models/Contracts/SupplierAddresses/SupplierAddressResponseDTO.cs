using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.SupplierAddressDTO_s;

public class SupplierAddressResponseDTO
{
    public int ID { get; set; }
    public int SupplierID { get; set; }
    public SupplierAddressType Type { get; set; }
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public string? StateOrProvince { get; set; }
    public required string PostalCode { get; set; }
    public required string CountryCode { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
