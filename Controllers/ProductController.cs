using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PATCH;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PUT.STOCK;
using InventoryManagementAPI.Repositories.ProductRepositories;
using InventoryManagementAPI.Models.CoreModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementAPI.Controllers
{
    [Route("api/products")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController (IProductService productService)
        {
            _productService = productService;
        }

        // === GET ===
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<SingleProductResponseDTO>>> GetProduct(int id)
        {
            var product = await _productService.GetSingleProduct(id);
            return StatusCode(product.StatusCode, product);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkProductResponseDTO>>>> GetAllProducts()
        {
            var products = await _productService.GetAllProducts();
            return StatusCode(products.StatusCode, products);
        }

        [HttpGet("~/api/categories/{categoryId:int}/products")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkProductResponseDTO>>>> GetProductsByCategory(int categoryId)
        {
            var products = await _productService.GetProductsByCategory(categoryId);
            return StatusCode(products.StatusCode, products);
        }

        [HttpGet("below-reorder-level")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkProductResponseDTO>>>> GetProductsBelowReorderLevel()
        {
            var products = await _productService.GetProductsBelowReorderLevel();
            return StatusCode(products.StatusCode, products);
        }


        // === POST ===
        [HttpPost]
        [Authorize(Policy =("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<SingleProductResponseDTO>>> AddProduct(CreateProductRequestDTO dto)
        {
            var addedProduct = await _productService.AddProduct(dto);

            return addedProduct.StatusCode switch
            {
                201 when addedProduct.Data is not null => CreatedAtAction(nameof(GetProduct), new { id = addedProduct.Data.ID }, addedProduct),
                _ => StatusCode(addedProduct.StatusCode, addedProduct)
            };
        }


        // === PUT ===
        [HttpPut("{id:int}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<SingleProductResponseDTO>>> UpdateProduct(int id, UpdateProductDetailsRequestDTO productDto)
        {
            var updatedProduct = await _productService.UpdateProductDetails(id, productDto);
            return StatusCode(updatedProduct.StatusCode, updatedProduct);
        }

  

        // === PATCH ===
        [HttpPatch("{id:int}/price")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<SingleProductResponseDTO>>> UpdateProductPrice (int id, UpdateProductPriceRequestDTO dto)
        {
            var updatedPriceProduct = await _productService.UpdateProductPrice(id, dto);
            return StatusCode(updatedPriceProduct.StatusCode, updatedPriceProduct);
        }

        [HttpPatch("{id:int}/reorder-level")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<SingleProductResponseDTO>>> UpdateProductReroderLevel(int id, UpdateProductReorderRequestDTO dto)
        {
            var updatedReOrderProduct = await _productService.UpdateProductReorderLevel(id, dto);
            return StatusCode(updatedReOrderProduct.StatusCode, updatedReOrderProduct);
        }



        // === SET ACTIVE/INACTIVE ===
        [HttpPatch("{id:int}/activate")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<SingleProductResponseDTO>>> ActivateProduct(int id, UpdateProductStatusRequestDTO dto)
        {
            var activatedProduct = await _productService.ActivateProduct(id, dto);
            return StatusCode(activatedProduct.StatusCode, activatedProduct);
        }

        [HttpPatch("{id:int}/deactivate")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<SingleProductResponseDTO>>> DeactivateProduct(int id, UpdateProductStatusRequestDTO dto)
        {
            var deactivatedProduct = await _productService.DeactivateProduct(id, dto);
            return StatusCode(deactivatedProduct.StatusCode, deactivatedProduct);
        }
    }
}
