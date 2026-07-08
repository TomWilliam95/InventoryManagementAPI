using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PUT.STOCK;

namespace InventoryManagementAPI.Repositories.ProductRepositories
{
    public interface IProductService
    {
        // === GET === \\
        Task<ApiResponse<SingleProductResponseDTO>> GetSingleProduct(int productId);
        Task<ApiResponse<IEnumerable<BulkProductResponseDTO>>> GetAllProducts();
        Task<ApiResponse<IEnumerable<BulkProductResponseDTO>>> GetProductsByCategory(int categoryId);
        Task<ApiResponse<IEnumerable<BulkProductResponseDTO>>> GetProductsBelowReorderLevel();

        // === POST === \\
        Task<ApiResponse<SingleProductResponseDTO>> AddProduct(CreateProductRequestDTO dto);

        // === PUT === \\
        Task<ApiResponse<SingleProductResponseDTO>> UpdateProductDetails(int id, UpdateProductDetailsRequestDTO dto);

        // === PATCH === \\
        Task<ApiResponse<SingleProductResponseDTO>> UpdateProductPrice(int id, UpdateProductPriceRequestDTO dto);
        Task<ApiResponse<SingleProductResponseDTO>> UpdateProductStockQuantity(int id, UpdateProductStockRequestDTO dto);
        Task<ApiResponse<SingleProductResponseDTO>> UpdateProductReorderLevel(int id, UpdateProductReorderRequestDTO dto);

        // === SET ACTIVE STATUS === \\
        Task<ApiResponse<SingleProductResponseDTO>> ActivateProduct(int id);
        Task<ApiResponse<SingleProductResponseDTO>> DeactivateProduct(int id);
    }
}
