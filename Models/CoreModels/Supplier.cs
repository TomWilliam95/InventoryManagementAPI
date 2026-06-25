using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.Models.CoreModels
{
    public class Supplier
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string PhoneContact { get; set; }
        public string EmailContact { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public DateOnly Created { get; set; }
        public DateOnly LastUpdated { get; set; }

        public virtual ICollection<Product>? Products { get; set; }
    }
}
