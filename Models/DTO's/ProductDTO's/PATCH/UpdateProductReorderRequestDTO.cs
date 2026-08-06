namespace InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PUT.STOCK
{
    public class UpdateProductReorderRequestDTO
    {
        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
