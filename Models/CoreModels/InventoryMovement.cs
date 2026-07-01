using InventoryManagementAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementAPI.Models.CoreModels
{
    public class InventoryMovement
    {
        [Key]
        public int ID { get; set; }

        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int QuantityBefore { get; set; }

        [Range(0, int.MaxValue)]
        public int QuantityAfter { get; set; }

        [EnumDataType(typeof(MovementType))]
        public MovementType Movement { get; set; }

        [Range(1, int.MaxValue)]
        public int UserID { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;

        [ForeignKey(nameof(ProductId))]
        public virtual Product? Product { get; set; }

        [ForeignKey(nameof(UserID))]
        public virtual User? User { get; set; }
    }
}
