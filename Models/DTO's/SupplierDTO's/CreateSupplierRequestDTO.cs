namespace InventoryManagementAPI.Models.DTO_s.SupplierDTO_s
{
    public class CreateSupplierRequestDTO
    {
        [Required]
        [StringLength(150)]
        public required string Name { get; set; }

        [Required]
        [StringLength(150)]
        public required string ContactName { get; set; }

        [Required]
        [Phone]
        [StringLength(30)]
        public required string PhoneContact { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public required string EmailContact { get; set; }

        [Required]
        [StringLength(300)]
        public required string Address { get; set; }

        public bool IsActive { get; set; }
    }
}
