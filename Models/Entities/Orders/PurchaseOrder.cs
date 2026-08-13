using InventoryManagementAPI.Models.CoreModels.UserModels;
using InventoryManagementAPI.Models.Enums;

namespace InventoryManagementAPI.Models.CoreModels.MovementModels
{
    public class PurchaseOrder
    {
        [Key]
        public int ID { get; set; }

        [Range(1, int.MaxValue)]
        public int SupplierID { get; set; }
        public Supplier Supplier { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int WarehouseID { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ReceivedDate { get; set; }

        public PurchaseOrderStatus Status { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        [StringLength(100)]
        public string? SupplierReference { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Range(1, int.MaxValue)]
        public int CreatedByUserID { get; set; }
        public User CreatedByUser { get; set; } = null!;

        [Timestamp]
        public byte[] RowVersion { get; set; } = [];

        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = [];
    }
}
