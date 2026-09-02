using InventoryManagementAPI.Models.Contracts.Products;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PATCH;
using InventoryManagementAPI.Models.Shared;

namespace InventoryManagementAPI.Repositories.ProductRepositories
{
    public interface IProductService
    {
        // === GET ===
        Task<ApiResponse<SingleProductResponseDTO>> GetSingleProduct(int productId, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<BulkProductResponseDTO>>> GetProducts(ProductQueryParameters query, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<BulkProductResponseDTO>>> GetProductsByCategory(int categoryId, ProductQueryParameters query, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<BulkProductResponseDTO>>> GetProductsBelowReorderLevel(ProductQueryParameters query, CancellationToken cancellationToken = default);

        // === POST ===
        Task<ApiResponse<SingleProductResponseDTO>> AddProduct(CreateProductRequestDTO dto, CancellationToken cancellationToken = default);

        // === PUT ===
        Task<ApiResponse<SingleProductResponseDTO>> UpdateProductDetails(int id, UpdateProductDetailsRequestDTO dto, CancellationToken cancellationToken = default);

        // === PATCH ===
        Task<ApiResponse<SingleProductResponseDTO>> UpdateProductPrice(int id, UpdateProductPriceRequestDTO dto, CancellationToken cancellationToken = default);

        // === SET ACTIVE STATUS ===
        Task<ApiResponse<SingleProductResponseDTO>> ActivateProduct(int id, UpdateProductStatusRequestDTO dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<SingleProductResponseDTO>> DeactivateProduct(int id, UpdateProductStatusRequestDTO dto, CancellationToken cancellationToken = default);
    }
}
