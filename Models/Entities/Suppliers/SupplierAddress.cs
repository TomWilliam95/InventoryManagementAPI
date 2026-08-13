using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.CoreModels.SupplierModels
{
    public class SupplierAddress
    {
        [Key]
        public int ID { get; set; }

        [Range(1, int.MaxValue)]
        public int SupplierID { get; set; }
        public Supplier Supplier { get; set; } = null!;

        [Required]
        public SupplierAddressType Type { get; set; }


        [Required]
        [StringLength(200)]
        public required string AddressLine1 { get; set; }

        [StringLength(200)]
        public string? AddressLine2 { get; set; }


        [Required]
        [StringLength(100)]
        public required string City { get; set; }

        [StringLength(100)]
        public string? StateOrProvince { get; set; }


        [Required]
        [StringLength(20)]
        public required string PostalCode { get; set; }


        // ISO 3166-1 alpha-2 code: AU, US, GB, etc.
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public required string CountryCode { get; set; }


        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; } = true;


        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;


        [Timestamp]
        public byte[] RowVersion { get; set; } = [];

        public ICollection<SupplierContact> SupplierContacts { get; set; } = new List<SupplierContact>();
    }
}
