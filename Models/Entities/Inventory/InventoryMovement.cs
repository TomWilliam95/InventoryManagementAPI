using InventoryManagementAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementAPI.Models.CoreModels.MovementModels
{
    public class InventoryMovement
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int InventoryStockID { get; set; }

        [ForeignKey(nameof(InventoryStockID))]
        public InventoryStock InventoryStock { get; set; } = null!;

        
        [Range(1, int.MaxValue)]
        public int UserID { get; set; }

        [ForeignKey(nameof(UserID))]
        public User User { get; set; } = null!;


        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int QuantityBefore { get; set; }

        [Range(0, int.MaxValue)]
        public int QuantityAfter { get; set; }

        
        
        [EnumDataType(typeof(MovementType))]
        public MovementType Movement { get; set; }

        

        [StringLength(500)]
        public required string Reason { get; set; }


        public DateTime Created { get; set; } = DateTime.UtcNow; 
    }
}
