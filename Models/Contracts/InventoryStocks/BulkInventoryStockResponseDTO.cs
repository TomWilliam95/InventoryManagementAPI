namespace InventoryManagementAPI.Models.DTO_s.InventoryStockDTO_s
{
    public class BulkInventoryStockResponseDTO
    {
        public int ID { get; set; }

        public int ProductID { get; set; }
        public required string ProductSku { get; set; }
        public required string ProductName { get; set; }

        public int WarehouseID { get; set; }
        public required string WarehouseName { get; set; }

        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public bool IsBelowReorderLevel { get; set; }
        public bool IsActive { get; set; }
    }
}
