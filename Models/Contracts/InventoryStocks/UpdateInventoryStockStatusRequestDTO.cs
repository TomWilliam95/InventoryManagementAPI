namespace InventoryManagementAPI.Models.DTO_s.InventoryStockDTO_s
{
    public class UpdateInventoryStockStatusRequestDTO
    {
        public bool IsActive { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = [];
    }
}
