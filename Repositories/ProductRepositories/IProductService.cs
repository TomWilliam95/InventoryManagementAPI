using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PUT.STOCK;

namespace InventoryManagementAPI.Repositories.ProductRepositories
{
    public interface IProductService
    {
        Task <ApiResponse<SingleProductResponseDTO>> AddProduct(CreateProductRequestDTO dto);
        Task <ApiResponse<SingleProductResponseDTO>> GetSingleProduct(int productId);
        Task<ApiResponse<IEnumerable<BulkProductResponseDTO>>> GetAllProducts();
        Task<ApiResponse<object>> DeleteProduct(int id);
        Task<ApiResponse<SingleProductResponseDTO>> UpdateProductDetails(int id, UpdateProductDetailsRequestDTO dto);
        Task<ApiResponse<SingleProductResponseDTO>> UpdateProductPrice(int id, UpdateProductPriceRequestDTO dto);
        Task<ApiResponse<SingleProductResponseDTO>> UpdateProductStockQuantity(int id, UpdateProductStockRequestDTO dto);
        Task<ApiResponse<SingleProductResponseDTO>> UpdateProductReorderLevel(int id, UpdateProductReorderRequestDTO dto);
        Task<ApiResponse<IEnumerable<BulkProductResponseDTO>>> GetProductsByCategory(int categoryId);
        Task<ApiResponse<IEnumerable<BulkProductResponseDTO>>> GetProductsBelowReorderLevel();
    }
}
