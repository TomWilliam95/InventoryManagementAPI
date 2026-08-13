namespace InventoryManagementAPI.Models.CoreModels.SupplierModels
{
    public class SupplierContact
    {
        [Key]
        public int ID { get; set; }

        [Range(1, int.MaxValue)]
        public int SupplierID { get; set; }
        public Supplier Supplier { get; set; } = null!;

        public int? SupplierAddressID { get; set; }
        public SupplierAddress? SupplierAddress { get; set; }

        [Required]
        [StringLength(150)]
        public required string Name { get; set; }

        [StringLength(100)]
        public string? JobTitle { get; set; }

        [EmailAddress]
        [StringLength(254)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(30)]
        public string? Phone { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = [];
    }
}
