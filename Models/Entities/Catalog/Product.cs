using InventoryManagementAPI.Models.CoreModels.MovementModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace InventoryManagementAPI.Models.CoreModels
{
    public class Product
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public required string Sku { get; set; }

        [Required]
        [StringLength(150)]
        public required string Name { get; set; }

        [Required]
        [StringLength(1000)]
        public required string Description { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 1000000, ErrorMessage = "Invalid Pricing")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; } = [];

        [ForeignKey(nameof(CategoryID))]
        public virtual Category? Category { get; set; }

        public virtual ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();

        public virtual ICollection<InventoryStock> InventoryStocks { get; set; } = [];
    }
}
