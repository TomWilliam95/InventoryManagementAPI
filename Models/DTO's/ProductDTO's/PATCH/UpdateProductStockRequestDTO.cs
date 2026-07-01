namespace InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PUT.STOCK
{
    public class UpdateProductStockRequestDTO
    {
        [Range(0, int.MaxValue)]
        public int QuantityInStock { get; set; }
    }
}
