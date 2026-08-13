namespace InventoryManagementAPI.Models.DTO_s.CategoryDTO_s
{
    public class SingleCategoryResponseDTO
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
