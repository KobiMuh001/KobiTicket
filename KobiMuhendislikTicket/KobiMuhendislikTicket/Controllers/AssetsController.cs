using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KobiMuhendislikTicket.Application.Services;
using System.Security.Claims;
using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Application.DTOs;

namespace KobiMuhendislikTicket.Controllers
{
    [Authorize] 
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly AssetService _assetService;
        public AssetsController(AssetService assetService) => _assetService = assetService;

#if DEBUG
        [HttpGet("debug-claims")]
        public IActionResult DebugClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(new { 
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                Claims = claims 
            });
        }
#endif

        [HttpGet("my-assets")]
        public async Task<IActionResult> GetMyAssets()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) 
                return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });

            if (!Guid.TryParse(userIdClaim.Value, out var tenantId))
                return Unauthorized(new { message = "Geçersiz kullanıcı kimliği." });

            var assets = await _assetService.GetMyAssetsAsync(tenantId);
            return Ok(assets);
        }

        // Admin: Tüm varlıkları listele
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllAssets()
        {
            var assets = await _assetService.GetAllAssetsAsync();
            return Ok(new { success = true, data = assets });
        }

        // Admin: Tek varlık detayı
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/{id}")]
        public async Task<IActionResult> GetAssetById(Guid id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null)
                return NotFound(new { success = false, message = "Varlık bulunamadı." });
            
            return Ok(new { success = true, data = asset });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin/add-asset")]
        public async Task<IActionResult> AddAsset(CreateAssetDto dto)
        {
            var asset = new Asset
            {
                ProductName = dto.ProductName,
                SerialNumber = dto.SerialNumber,
                TenantId = dto.TenantId,
                WarrantyEndDate = dto.WarrantyEndDate ?? DateTime.Now.AddYears(2), 
                Status = "Aktif"
            };

            var (success, message) = await _assetService.CreateAssetAsync(asset);
            if (!success)
                return BadRequest(new { success = false, message = message });

            return Ok(new { success = true, message = message, data = asset });
        }

        // Admin: Varlık güncelle
        [Authorize(Roles = "Admin")]
        [HttpPut("admin/{id}")]
        public async Task<IActionResult> UpdateAsset(Guid id, UpdateAssetDto dto)
        {
            var (success, message) = await _assetService.UpdateAssetAsync(id, dto);
            if (!success)
                return BadRequest(new { success = false, message = message });
            
            return Ok(new { success = true, message = message });
        }

        // Admin: Varlık sil
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> DeleteAsset(Guid id)
        {
            var result = await _assetService.DeleteAssetAsync(id);
            if (!result)
                return NotFound(new { success = false, message = "Varlık bulunamadı." });
            
            return Ok(new { success = true, message = "Varlık başarıyla silindi." });
        }
    }
}