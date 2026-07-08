namespace InventoryManagementAPI.Models.DTO_s.CategoryDTO_s
{
    public class UpdateCategoryDetailsRequestDTO
    {
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
