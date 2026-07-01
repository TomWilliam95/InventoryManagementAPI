namespace InventoryManagementAPI.Models.DTO_s.SupplierDTO_s
{
    public class CreateSupplierRequestDTO
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        [StringLength(150)]
        public string ContactName { get; set; }

        [Required]
        [Phone]
        [StringLength(30)]
        public string PhoneContact { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string EmailContact { get; set; }

        [Required]
        [StringLength(300)]
        public string Address { get; set; }

        public bool IsActive { get; set; }
    }
}
