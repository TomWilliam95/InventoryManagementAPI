using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.Models.DTO_s.ProductDTO_s
{
    public class SingleProductResponseDTO
    {
        public int ID { get; set; }
        public required string Sku { get; set; }
        public  required string Name { get; set; }
        public required string Description { get; set; }
        public int CategoryID { get; set; }
        public required string CategoryName { get; set; }
        public int QuantityInStock { get; set; }
        public int ReorderLevel { get; set; }
        public decimal Price { get; set; }
        public int SupplierID { get; set; }
        public required string SupplierName { get; set; } 
        public bool IsActive { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
