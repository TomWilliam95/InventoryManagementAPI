namespace InventoryManagementAPI.Models.DTO_s.SupplierDTO_s
{
    public class SupplierResponseDTO
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required string ContactName { get; set; }
        public required string PhoneContact { get; set; }
        public required string EmailContact { get; set; }
        public bool IsActive { get; set; }
    }
}
