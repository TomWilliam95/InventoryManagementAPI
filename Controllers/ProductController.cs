using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PATCH;
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
        public async Task<ActionResult<ApiResponse<SingleProductResponseDTO>>> GetProduct(int id, CancellationToken cancellationToken = default)
        {
            var product = await _productService.GetSingleProduct(id, cancellationToken);
            return StatusCode(product.StatusCode, product);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkProductResponseDTO>>>> GetAllProducts(CancellationToken cancellationToken = default)
        {
            var products = await _productService.GetAllProducts(cancellationToken);
            return StatusCode(products.StatusCode, products);
        }

        [HttpGet("~/api/categories/{categoryId:int}/products")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkProductResponseDTO>>>> GetProductsByCategory(int categoryId, CancellationToken cancellationToken = default)
        {
            var products = await _productService.GetProductsByCategory(categoryId, cancellationToken);
            return StatusCode(products.StatusCode, products);
        }

        [HttpGet("below-reorder-level")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BulkProductResponseDTO>>>> GetProductsBelowReorderLevel(CancellationToken cancellationToken = default)
        {
            var products = await _productService.GetProductsBelowReorderLevel(cancellationToken);
            return StatusCode(products.StatusCode, products);
        }


        // === POST ===
        [HttpPost]
        [Authorize(Policy =("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<SingleProductResponseDTO>>> AddProduct(CreateProductRequestDTO dto, CancellationToken cancellationToken = default)
        {
            var addedProduct = await _productService.AddProduct(dto, cancellationToken);

            return addedProduct.StatusCode switch
            {
                201 when addedProduct.Data is not null => CreatedAtAction(nameof(GetProduct), new { id = addedProduct.Data.ID }, addedProduct),
                _ => StatusCode(addedProduct.StatusCode, addedProduct)
            };
        }


        // === PUT ===
        [HttpPut("{id:int}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<SingleProductResponseDTO>>> UpdateProduct(int id, UpdateProductDetailsRequestDTO productDto, CancellationToken cancellationToken = default)
        {
            var updatedProduct = await _productService.UpdateProductDetails(id, productDto, cancellationToken);
            return StatusCode(updatedProduct.StatusCode, updatedProduct);
        }

  

        // === PATCH ===
        [HttpPatch("{id:int}/price")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<SingleProductResponseDTO>>> UpdateProductPrice (int id, UpdateProductPriceRequestDTO dto, CancellationToken cancellationToken = default)
        {
            var updatedPriceProduct = await _productService.UpdateProductPrice(id, dto, cancellationToken);
            return StatusCode(updatedPriceProduct.StatusCode, updatedPriceProduct);
        }

        // === SET ACTIVE/INACTIVE ===
        [HttpPatch("{id:int}/activate")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<SingleProductResponseDTO>>> ActivateProduct(int id, UpdateProductStatusRequestDTO dto, CancellationToken cancellationToken = default)
        {
            var activatedProduct = await _productService.ActivateProduct(id, dto, cancellationToken);
            return StatusCode(activatedProduct.StatusCode, activatedProduct);
        }

        [HttpPatch("{id:int}/deactivate")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<ApiResponse<SingleProductResponseDTO>>> DeactivateProduct(int id, UpdateProductStatusRequestDTO dto, CancellationToken cancellationToken = default)
        {
            var deactivatedProduct = await _productService.DeactivateProduct(id, dto, cancellationToken);
            return StatusCode(deactivatedProduct.StatusCode, deactivatedProduct);
        }
    }
}
