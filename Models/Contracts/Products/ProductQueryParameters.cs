
namespace InventoryManagementAPI.Models.Contracts.Products
{
    public class ProductQueryParameters: IValidatableObject
    {
        [Range(1, int.MaxValue, ErrorMessage ="Page must be at least 1.")]
        public int Page { get; set; } = 1;

        
        [Range(1,100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 20;

        
        [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters.")]
        public string? Search { get; set; }


        [StringLength(50, ErrorMessage = "SortBy cannot exceed 50 characters.")]
        public int? CategoryId { get; set; }

        public bool? IsActive { get; set; }


        [Range(typeof(decimal), "0", "9999999999999999.99", ErrorMessage = "MinPrice must be a non-negative value.")]
        public decimal? MinPrice { get; set; }


        [Range(typeof(decimal), "0", "9999999999999999.99", ErrorMessage = "MaxPrice must be a non-negative value.")]
        public decimal? MaxPrice { get; set; }


        [Required]
        [RegularExpression("^(name| sku| price|created| id)$", ErrorMessage = "SortBy must be one of the following: name, price, createdDate.")]
        public string SortBy { get; set; } = "name";

        [Required]
        [RegularExpression("^(asc|desc)$", ErrorMessage = "SortDirection must be either 'asc' or 'desc'.")]
        public string SortDirection { get; set; } = "asc";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
            {
                yield return new ValidationResult("MinPrice cannot be greater than MaxPrice.", [nameof(MinPrice), nameof(MaxPrice)] );
            }
        }
    }
}
