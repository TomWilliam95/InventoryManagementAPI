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
        public string Sku { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }

        [Range(0, int.MaxValue)]
        public int QuantityInStock { get; set; }

        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 1000000, ErrorMessage = "Invalid Pricing")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue)]
        public int SupplierID { get; set; }
        public bool IsActive { get; set; }
        public DateOnly Created { get; set; }
        public DateTime Updated { get; set; }

        [ForeignKey(nameof(CategoryID))]
        public virtual Category? Category { get; set; }

        [ForeignKey(nameof(SupplierID))]
        public virtual Supplier? Supplier { get; set; }

        public virtual ICollection<InventoryMovement>? InventoryMovements { get; set; }
    }
}
