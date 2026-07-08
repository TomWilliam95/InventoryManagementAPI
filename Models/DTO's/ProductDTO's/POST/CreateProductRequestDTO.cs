namespace InventoryManagementAPI.Models.DTO_s.ProductDTO_s
{
    public class CreateProductRequestDTO
    {
        [Required]
        [StringLength(50)]
        public required string Sku { get; set; }

        [Required]
        [StringLength(150)]
        public required string Name { get; set; }

        [Required]
        [StringLength(1000)]
        public required string Description { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }

        [Range(0, int.MaxValue)]
        public int QuantityInStock { get; set; }

        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }

        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue)]
        public int SupplierID { get; set; }

        public bool IsActive { get; set; }
    }
}
