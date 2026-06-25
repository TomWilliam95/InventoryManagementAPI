namespace InventoryManagementAPI.Models.DTO_s.ProductDTO_s
{
    public class CreateProductRequestDTO
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public int QuantityInStock { get; set; }
        public int ReorderLevel { get; set; }
        public decimal Price { get; set; }
        public int SupplierID { get; set; }
        public bool IsActive { get; set; }
    }
}
