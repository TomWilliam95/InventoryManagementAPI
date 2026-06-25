namespace InventoryManagementAPI.Models.DTO_s.ProductDTO_s
{
    public class BulkProductResponseDTO
    {
        public int ID { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public int QuantityInStock { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
