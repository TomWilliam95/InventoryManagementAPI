namespace InventoryManagementAPI.Models.DTO_s.CategoryDTO_s
{
    public class UpdateCategoryStatusRequestDTO
    {
        public bool IsActive { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
