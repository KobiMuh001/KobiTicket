using KobiMuhendislikTicket.Application.Services;
using KobiMuhendislikTicket.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KobiMuhendislikTicket.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(new { success = true, data = products });
        }

        [HttpPost("admin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            var (success, message) = await _productService.CreateProductAsync(dto);
            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new { success = true, message });
        }

        [HttpPut("admin/{productId}")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] UpdateProductDto dto)
        {
            var (success, message) = await _productService.UpdateProductAsync(productId, dto);
            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new { success = true, message });
        }

        [HttpDelete("admin/{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var (success, message) = await _productService.DeleteProductAsync(productId);
            if (!success)
                return NotFound(new { success = false, message });

            return Ok(new { success = true, message });
        }

        [HttpGet("admin/{productId}/tenants")]
        public async Task<IActionResult> GetProductTenants(int productId)
        {
            var product = await _productService.GetProductTenantsAsync(productId);
            if (product == null)
                return NotFound(new { success = false, message = "Ürün bulunamadı." });

            return Ok(new { success = true, data = product });
        }

        [HttpPost("admin/{productId}/tenants/{tenantId}")]
        public async Task<IActionResult> AssignProductToTenant(int productId, int tenantId, [FromBody] AssignProductTenantDto dto)
        {
            var (success, message) = await _productService.AssignProductToTenantAsync(productId, tenantId, dto);
            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new { success = true, message });
        }

        [HttpPut("admin/{productId}/tenants/{tenantId}")]
        public async Task<IActionResult> UpdateProductTenant(int productId, int tenantId, [FromBody] UpdateProductTenantDto dto)
        {
            var (success, message) = await _productService.UpdateProductTenantAsync(productId, tenantId, dto);
            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new { success = true, message });
        }

        [HttpDelete("admin/{productId}/tenants/{tenantId}")]
        public async Task<IActionResult> RemoveProductFromTenant(int productId, int tenantId)
        {
            var (success, message) = await _productService.RemoveProductFromTenantAsync(productId, tenantId);
            if (!success)
                return NotFound(new { success = false, message });

            return Ok(new { success = true, message });
        }

        [AllowAnonymous]
        [HttpGet("tenant/{tenantId}")]
        public async Task<IActionResult> GetTenantProducts(int tenantId)
        {
            var products = await _productService.GetTenantProductsAsync(tenantId);
            return Ok(new { success = true, data = products });
        }
    }
}
