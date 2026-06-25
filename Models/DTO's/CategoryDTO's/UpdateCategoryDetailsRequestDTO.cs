namespace InventoryManagementAPI.Models.DTO_s.CategoryDTO_s
{
    public class UpdateCategoryDetailsRequestDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
