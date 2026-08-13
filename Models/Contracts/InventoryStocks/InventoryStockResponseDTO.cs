namespace InventoryManagementAPI.Models.DTO_s.InventoryStockDTO_s
{
    public class InventoryStockResponseDTO
    {
        public int ID { get; set; }

        public int ProductID { get; set; }
        public required string ProductSku { get; set; }
        public required string ProductName { get; set; }

        public int WarehouseID { get; set; }
        public required string WarehouseName { get; set; }

        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }

        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
