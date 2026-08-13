namespace InventoryManagementAPI.Models.DTO_s.InventoryStockDTO_s
{
    public class UpdateReorderLevelRequestDTO
    {
        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = [];
    }
}
