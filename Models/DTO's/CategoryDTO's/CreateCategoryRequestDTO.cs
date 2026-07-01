namespace InventoryManagementAPI.Models.DTO_s.CategoryDTO_s
{
    public class CreateCategoryRequestDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
