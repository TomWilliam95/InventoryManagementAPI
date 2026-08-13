namespace InventoryManagementAPI.Models.DTO_s.CategoryDTO_s
{
    public class BulkCategoryResponseDTO
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
