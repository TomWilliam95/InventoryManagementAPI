using InventoryManagementAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.Models.CoreModels
{
    public class InventoryMovement
    {
        [Key]
        public int ID { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int QuantityBefore { get; set; }
        public int QuantityAfter { get; set; }
        public MovementType Movement { get; set; }
        public int UserID { get; set; }
        public string Reason { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;

        public virtual Product? Product { get; set; }
        public virtual User? User { get; set; }
    }
}
