using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementAPI.Models.CoreModels.MovementModels
{
    public class SalesOrderItem
    {
        [Key]
        public int ID { get; set; }

        [Range(1, int.MaxValue)]
        public int SalesOrderID { get; set; }
        public SalesOrder SalesOrder { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int ProductID { get; set; }
        public Product Product { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int QuantityOrdered { get; set; }

        [Range(0, int.MaxValue)]
        public int QuantityDispatched { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal DiscountAmount { get; set; }
    }
}
