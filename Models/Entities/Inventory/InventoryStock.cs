using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementAPI.Models.CoreModels
{
    public class InventoryStock
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ProductID { get; set; }
        public Product Product { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue)]
        public int WarehouseID { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        
        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }

        
        [Required]
        public DateTime Created { get; set; }
        
        [Required]
        public DateTime Updated { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [Timestamp]
        public byte[] RowVersion { get; set; } = [];

        public ICollection<InventoryMovement> InventoryMovements { get; set; } = [];
    }
}
