using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace InventoryManagementAPI.Models.CoreModels
{
    public class Product
    {
        [Key]
        public int ID { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CategoryID { get; set; }
        public int QuantityInStock { get; set; }
        public int ReorderLevel { get; set; }
        [Range(0.01, 1000000, ErrorMessage ="Invalid Pricing")]
        public decimal Price { get; set; }
        public int SupplierID { get; set; }
        public bool IsActive { get; set; }
        public DateOnly Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Category? Category { get; set; }
        public virtual Supplier? Supplier { get; set; }

        public virtual ICollection<InventoryMovement>? InventoryMovements { get; set; }
    }
}
