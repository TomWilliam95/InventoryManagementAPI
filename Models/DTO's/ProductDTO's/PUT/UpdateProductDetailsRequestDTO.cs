using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.Models.DTO_s.ProductDTO_s
{
    public class UpdateProductDetailsRequestDTO
    {
        [Required]
        [StringLength(8, MinimumLength = 8)]
        public required string Sku { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 5)]
        public required string Name { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public required string Description { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }

        [Range(1, int.MaxValue)]
        public int SupplierID { get; set; }
    }
}
