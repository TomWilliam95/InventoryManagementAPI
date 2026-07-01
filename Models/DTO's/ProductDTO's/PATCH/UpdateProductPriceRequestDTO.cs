namespace InventoryManagementAPI.Models.DTO_s.ProductDTO_s
{
    public class UpdateProductPriceRequestDTO
    {
        [Range(0.01, 1000000)]
        public decimal Price { get; set; }
    }
}
