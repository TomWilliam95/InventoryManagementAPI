using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.Models.CoreModels
{
    public class Supplier
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        [StringLength(150)]
        public string ContactName { get; set; }

        [Required]
        [Phone]
        [StringLength(30)]
        public string PhoneContact { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string EmailContact { get; set; }

        [Required]
        [StringLength(300)]
        public string Address { get; set; }

        public bool IsActive { get; set; }
        public DateOnly Created { get; set; }
        public DateOnly LastUpdated { get; set; }

        public virtual ICollection<Product>? Products { get; set; }
    }
}
