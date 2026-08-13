using InventoryManagementAPI.Models.CoreModels.UserModels;
using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.CoreModels.MovementModels
{
    public class SalesOrder
    {
        [Key]
        public int ID { get; set; }

        [Range(1, int.MaxValue)]
        public int CustomerID { get; set; }
        public Customer Customer { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int WarehouseID { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public SalesOrderStatus Status { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? DispatchedDate { get; set; }

        [StringLength(100)]
        public string? CustomerReference { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Range(1, int.MaxValue)]
        public int CreatedByUserID { get; set; }
        public User CreatedByUser { get; set; } = null!;

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = [];

        public ICollection<SalesOrderItem> Items { get; set; } = [];
    }
}
