using Azure;
using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s;
using InventoryManagementAPI.Models.DTO_s.ProductDTO_s.PUT.STOCK;
using InventoryManagementAPI.Repositories.ProductRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementAPI.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController (IProductService productService)
        {
            _productService = productService;
        }

        // === POST === \\
        [HttpPost]
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

        // === GET === \\
        [HttpGet("{id}")]
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

        [HttpGet]
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

        [HttpGet("category/{categoryId}")]
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

        [HttpGet("below-reorder-level")]
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

        // === DELETE === \\
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deletedProduct = await _productService.DeleteProduct(id);

            return deletedProduct.StatusCode switch
            {
                204 => Ok(deletedProduct.Data),
                404 => NotFound(deletedProduct.Message),
                500 => StatusCode(500, deletedProduct.Message),
                _ => StatusCode(deletedProduct.StatusCode, deletedProduct)
            };
        }


        // === PUT === \\
        [HttpPut("{id}")]
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
        [HttpPatch("{id}/price")]
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

        [HttpPatch("{id}/stock")]
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

        [HttpPatch("{id}/reorder-level")]
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
    }
}
