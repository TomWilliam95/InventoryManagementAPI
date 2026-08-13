namespace InventoryManagementAPI.Models.DTO_s.InventoryStockDTO_s
{
    public class CreateInventoryStockRequestDTO
    {
        [Range(1, int.MaxValue)]
        public int ProductID { get; set; }

        [Range(1, int.MaxValue)]
        public int WarehouseID { get; set; }

        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }
    }
}
