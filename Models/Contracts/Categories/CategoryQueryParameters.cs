namespace InventoryManagementAPI.Models.Contracts.Categories
{
    public class CategoryQueryParameters
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
        public int Page { get; set; } = 1;


        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 20;


        [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters.")]
        public string? Search { get; set; }

        public bool? IsActive { get; set; }

        [Required]
        [RegularExpression("^(name|created|id)$", ErrorMessage = "SortBy must be one of the following: name, price, createdDate.")]
        public string SortBy { get; set; } = "name";

        [Required]
        [RegularExpression("^(asc|desc)$", ErrorMessage = "SortDirection must be either 'asc' or 'desc'.")]
        public string SortDirection { get; set; } = "asc";
    }
}
