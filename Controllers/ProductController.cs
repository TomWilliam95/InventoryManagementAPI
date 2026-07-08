using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PUT.STOCK;
using InventoryManagementAPI.Repositories.ProductRepositories;
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

        // === GET === \\
        [HttpGet("Product/{id}")]
        public async Task<ActionResult<SingleProductResponseDTO>> GetProduct(int id)
        {
            var product = await _productService.GetSingleProduct(id);

            return product.StatusCode switch
            {
                200 => Ok(product.Data),
                404 => NotFound(product.Message),
                500 => StatusCode(500,product.Message),
                _ => StatusCode(product.StatusCode, product)
            };
        }

        [HttpGet("AllProducts")]
        public async Task<ActionResult<IEnumerable<BulkProductResponseDTO>>> GetAllProducts()
        {
            var products = await _productService.GetAllProducts();

            return products.StatusCode switch
            {
                200 => Ok(products.Data),
                404 => NotFound(products.Message),
                500 => StatusCode(500, products.Message),
                _ => StatusCode(products.StatusCode, products)
            };
        }

        [HttpGet("ProductsByCategory/{categoryId}")]
        public async Task<ActionResult<IEnumerable<BulkProductResponseDTO>>> GetProductsByCategory(int categoryId)
        {
            var products = await _productService.GetProductsByCategory(categoryId);
            return products.StatusCode switch
            {
                200 => Ok(products.Data),
                404 => NotFound(products.Message),
                500 => StatusCode(500, products.Message),
                _ => StatusCode(products.StatusCode, products)
            };
        }

        [HttpGet("ProductsBelowReorderLevel")]
        public async Task<ActionResult<IEnumerable<BulkProductResponseDTO>>> GetProductsBelowReorderLevel()
        {
            var products = await _productService.GetProductsBelowReorderLevel();
            return products.StatusCode switch
            {
                200 => Ok(products.Data),
                404 => NotFound(products.Message),
                500 => StatusCode(500, products.Message),
                _ => StatusCode(products.StatusCode, products)
            };
        }

        // === POST === \\
        [HttpPost("AddProduct")]
        [Authorize(Policy =("AdminOrManager"))]
        public async Task<ActionResult<SingleProductResponseDTO>> AddProduct(CreateProductRequestDTO dto)
        {
            var addedProduct = await _productService.AddProduct(dto);

            return addedProduct.StatusCode switch
            {
                201 => CreatedAtAction(nameof(GetProduct), new { id = addedProduct.Data.ID }, addedProduct),
                400 => BadRequest(addedProduct.Message),
                404 => NotFound(addedProduct.Message),
                500 => StatusCode(500, addedProduct.Message),
                _ => StatusCode(addedProduct.StatusCode, addedProduct)
            };
        }

        // === PUT === \\
        [HttpPut("UpdateProduct/{id}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<SingleProductResponseDTO>> UpdateProduct(int id, UpdateProductDetailsRequestDTO productDto)
        {
            var updatedProduct = await _productService.UpdateProductDetails(id, productDto);

            return updatedProduct.StatusCode switch
            {
                200 => Ok(updatedProduct.Data),
                400 => BadRequest(updatedProduct.Message),
                404 => NotFound(updatedProduct.Message),
                500 => StatusCode(500, updatedProduct.Message),
                _ => StatusCode(updatedProduct.StatusCode, updatedProduct)
            };
        }

  
        // === PATCH === \\
        [HttpPatch("UpdatePrice/{id}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<SingleProductResponseDTO>> UpdateProductPrice (int id, UpdateProductPriceRequestDTO dto)
        {
            var updatedPriceProduct = await _productService.UpdateProductPrice(id, dto);

            return updatedPriceProduct.StatusCode switch
            {
                200 => Ok(updatedPriceProduct.Data),
                400 => BadRequest(updatedPriceProduct.Message),
                404 => NotFound(updatedPriceProduct.Message),
                500 => StatusCode(500, updatedPriceProduct.Message),
                _ => StatusCode(updatedPriceProduct.StatusCode, updatedPriceProduct)
            };
        }

        [HttpPatch("UpdateStock/{id}")]
        public async Task<ActionResult<SingleProductResponseDTO>> UpdateProductStock(int id, UpdateProductStockRequestDTO dto)
        {
            var updatedStockProduct = await _productService.UpdateProductStockQuantity(id, dto);

            return updatedStockProduct.StatusCode switch
            {
                200 => Ok(updatedStockProduct.Data),
                400 => BadRequest(updatedStockProduct.Message),
                404 => NotFound(updatedStockProduct.Message),
                500 => StatusCode(500, updatedStockProduct.Message),
                _ => StatusCode(updatedStockProduct.StatusCode, updatedStockProduct)
            };
        }

        [HttpPatch("UpdateProductReorderLevel/{id}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<SingleProductResponseDTO>> UpdateProductReroderLevel(int id, UpdateProductReorderRequestDTO dto)
        {
            var updatedReOrderProduct = await _productService.UpdateProductReorderLevel(id, dto);

            return updatedReOrderProduct.StatusCode switch
            {
                200 => Ok(updatedReOrderProduct.Data),
                400 => BadRequest(updatedReOrderProduct.Message),
                404 => NotFound(updatedReOrderProduct.Message),
                500 => StatusCode(500, updatedReOrderProduct.Message),
                _ => StatusCode(updatedReOrderProduct.StatusCode, updatedReOrderProduct)
            };
        }

        // === SET ACTIVE/INACTIVE === \\
        [HttpPatch("ActivateProduct/{id}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<SingleProductResponseDTO>> ActivateProduct(int id)
        {
            var activatedProduct = await _productService.ActivateProduct(id);
            return activatedProduct.StatusCode switch
            {
                200 => Ok(activatedProduct.Data),
                400 => BadRequest(activatedProduct.Message),
                404 => NotFound(activatedProduct.Message),
                500 => StatusCode(500, activatedProduct.Message),
                _ => StatusCode(activatedProduct.StatusCode, activatedProduct)
            };
        }

        [HttpPatch("DeactivateProduct/{id}")]
        [Authorize(Policy = ("AdminOrManager"))]
        public async Task<ActionResult<SingleProductResponseDTO>> DeactivateProduct(int id)
        {
            var deactivatedProduct = await _productService.DeactivateProduct(id);
            return deactivatedProduct.StatusCode switch
            {
                200 => Ok(deactivatedProduct.Data),
                400 => BadRequest(deactivatedProduct.Message),
                404 => NotFound(deactivatedProduct.Message),
                500 => StatusCode(500, deactivatedProduct.Message),
                _ => StatusCode(deactivatedProduct.StatusCode, deactivatedProduct)
            };
        }
    }
}
