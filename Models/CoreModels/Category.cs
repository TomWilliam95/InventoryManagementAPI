using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.Models.CoreModels
{
    public class Category
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateOnly Created { get; set; }
        public DateOnly Updated { get; set; }

        public virtual ICollection<Product>? Products { get; set; }
    }
}
