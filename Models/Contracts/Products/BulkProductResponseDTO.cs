namespace InventoryManagementAPI.Models.DTO_s.ProductDTO_s
{
    public class BulkProductResponseDTO
    {
        public int ID { get; set; }
        public required string Sku { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
