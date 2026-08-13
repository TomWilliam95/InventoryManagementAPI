namespace InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PATCH
{
    public class UpdateProductStatusRequestDTO
    {
        public bool IsActive { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
