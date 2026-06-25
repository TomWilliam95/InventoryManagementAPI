namespace InventoryManagementAPI.Models.DTO_s.SupplierDTO_s
{
    public class CreateSupplierRequestDTO
    {
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string PhoneContact { get; set; }
        public string EmailContact { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
    }
}
