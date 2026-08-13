namespace InventoryManagementAPI.Models.DTO_s.SupplierDTO_s
{
    public class CreateSupplierRequestDTO
    {
        [Required]
        [StringLength(150)]
        public required string Name { get; set; }

        [StringLength(100)]
        public string? TaxNumber { get; set; }

        [StringLength(300)]
        [Url]
        public string? Website { get; set; }

        public bool IsActive { get; set; }
    }
}
