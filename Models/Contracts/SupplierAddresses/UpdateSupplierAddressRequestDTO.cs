using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.DTO_s.SupplierAddressDTO_s;

public class UpdateSupplierAddressRequestDTO
{
    [EnumDataType(typeof(SupplierAddressType))]
    public SupplierAddressType Type { get; set; }
    [Required, StringLength(200)]
    public required string AddressLine1 { get; set; }
    [StringLength(200)]
    public string? AddressLine2 { get; set; }
    [Required, StringLength(100)]
    public required string City { get; set; }
    [StringLength(100)]
    public string? StateOrProvince { get; set; }
    [Required, StringLength(20)]
    public required string PostalCode { get; set; }
    [Required, StringLength(2, MinimumLength = 2)]
    public required string CountryCode { get; set; }
    public bool IsPrimary { get; set; }

    [Required]
    public byte[] RowVersion { get; set; } = [];
}
