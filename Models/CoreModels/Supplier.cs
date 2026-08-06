using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.Models.CoreModels
{
    public class Supplier
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(150)]
        public required string Name { get; set; }

        [Required]
        [StringLength(150)]
        public required string ContactName { get; set; }

        [Required]
        [Phone]
        [StringLength(30)]
        public required string PhoneContact { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public required string EmailContact { get; set; }

        [Required]
        [StringLength(300)]
        public required string Address { get; set; }

        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; } = [];

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
