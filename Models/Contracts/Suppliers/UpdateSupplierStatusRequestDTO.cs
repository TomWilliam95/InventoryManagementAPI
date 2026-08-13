namespace InventoryManagementAPI.Models.DTO_s.SupplierDTO_s
{
    public class UpdateSupplierStatusRequestDTO
    {
        public bool IsActive { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
