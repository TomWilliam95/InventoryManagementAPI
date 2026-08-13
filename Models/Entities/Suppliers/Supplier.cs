using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.Models.CoreModels.SupplierModels
{
    public class Supplier
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(150)]
        public required string Name { get; set; }


        [StringLength(100)] 
        public string? TaxNumber { get; set; }

        [StringLength(300)]
        [Url]
        public string? Website { get; set; }

        public bool IsActive { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; } = [];

        public ICollection<SupplierContact> SupplierContacts { get; set; } = new List<SupplierContact>();
        public ICollection<SupplierAddress> SupplierAddresses { get; set; } = new List<SupplierAddress>();
        public ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();
    }
}
