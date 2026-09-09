namespace InventoryManagementAPI.Models.Contracts.InventoryStocks
{
    public class InventoryStockQueryParameters
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 20;

        [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters.")]
        public string? Search { get; set; }


        public bool? IsActive { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive integer.")]
        public int? ProductId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "WarehouseId must be a positive integer.")]
        public int? WarehouseId { get; set; }

        public bool? BelowReorderLevel { get; set; }


        [Required]
        [RegularExpression("^(product|warehouse|created|id)$", ErrorMessage = "SortBy must be one of the following: product, warehouse, created, id.")]
        public string SortBy { get; set; } = "product";

        [Required]
        [RegularExpression("^(asc|desc)$", ErrorMessage = "SortDirection must be either 'asc' or 'desc'.")]
        public string SortDirection { get; set; } = "asc";
    }
}
