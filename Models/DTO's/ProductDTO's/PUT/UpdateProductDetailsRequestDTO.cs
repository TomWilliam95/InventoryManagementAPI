using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.Models.DTO_s.ProductDTO_s
{
    public class UpdateProductDetailsRequestDTO
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public int SupplierID { get; set; }
        public bool IsActive { get; set; }
    }
}
