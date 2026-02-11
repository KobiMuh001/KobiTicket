using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using KobiMuhendislikTicket.Application.Services;
using KobiMuhendislikTicket.Application.DTOs;

namespace KobiMuhendislikTicket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly TenantService _tenantService;
        private readonly AuthService _authService;

        public TenantsController(TenantService tenantService, AuthService authService)
        {
            _tenantService = tenantService;
            _authService = authService;
        }

        // ==================== ADMIN İŞLEMLERİ ====================

        /// <summary>
        /// Yeni müşteri oluşturur (Admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/create-tenant")]
        public async Task<IActionResult> Register(CreateTenantDto dto)
        {
            var result = await _tenantService.CreateTenantAsync(dto);
            if (result != "Ok") return BadRequest(new { success = false, message = result });

            return Ok(new { success = true, message = "Firma başarıyla oluşturuldu." });
        }

        // ==================== MÜŞTERİ KENDİ İŞLEMLERİ ====================

        /// <summary>
        /// Müşteri kendi bilgilerini görüntüler
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var tenantId = GetCurrentTenantId();
            if (tenantId == null)
                return Unauthorized(new { success = false, message = "Kullanıcı kimliği bulunamadı." });

            var tenant = await _tenantService.GetByIdAsync(tenantId.Value);
            if (tenant == null)
                return NotFound(new { success = false, message = "Müşteri bulunamadı." });

            return Ok(new
            {
                success = true,
                data = new
                {
                    tenant.Id,
                    tenant.CompanyName,
                    tenant.TaxNumber,
                    tenant.Email,
                    tenant.PhoneNumber,
                    tenant.CreatedDate
                }
            });
        }

        /// <summary>
        /// Müşteri kendi bilgilerini günceller
        /// </summary>
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateTenantDto dto)
        {
            var tenantId = GetCurrentTenantId();
            if (tenantId == null)
                return Unauthorized(new { success = false, message = "Kullanıcı kimliği bulunamadı." });

            var result = await _tenantService.UpdateTenantAsync(tenantId.Value, dto);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "Bilgileriniz güncellendi." });
        }

        
        [Authorize]
        [HttpPost("me/change-password")]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordDto dto)
        {
            var tenantId = GetCurrentTenantId();
            if (tenantId == null)
                return Unauthorized(new { success = false, message = "Kullanıcı kimliği bulunamadı." });

            var result = await _tenantService.ChangePasswordAsync(tenantId.Value, dto);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "Şifreniz başarıyla değiştirildi." });
        }

        // ==================== YARDIMCI METODLAR ====================

        private int? GetCurrentTenantId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return null;

            if (int.TryParse(claim.Value, out var tenantId))
                return tenantId;

            return null;
        }
    }
}