using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _environment;

        public TenantsController(TenantService tenantService, AuthService authService, IWebHostEnvironment environment)
        {
            _tenantService = tenantService;
            _authService = authService;
            _environment = environment;
        }

        // ==================== ADMIN İŞLEMLERİ ====================
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/create-tenant")]
        public async Task<IActionResult> Register(CreateTenantDto dto)
        {
            var result = await _tenantService.CreateTenantAsync(dto);
            if (result != "Ok") return BadRequest(new { success = false, message = result });

            return Ok(new { success = true, message = "Firma başarıyla oluşturuldu." });
        }

        // ==================== MÜŞTERİ KENDİ İŞLEMLERİ ====================

        
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
                    tenant.Username,
                    tenant.PhoneNumber,
                    tenant.LogoUrl,
                    tenant.CreatedDate
                }
            });
        }
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
        [HttpPost("me/upload-logo")]
        public async Task<IActionResult> UploadMyLogo(IFormFile file)
        {
            var tenantId = GetCurrentTenantId();
            if (tenantId == null)
                return Unauthorized(new { success = false, message = "Kullanıcı kimliği bulunamadı." });

            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "Dosya seçilmedi." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new { success = false, message = "Yalnızca resim dosyaları (jpg, png, gif, webp) yüklenebilir." });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { success = false, message = "Dosya boyutu 5MB'ı geçemez." });

            var tenant = await _tenantService.GetByIdAsync(tenantId.Value);
            if (tenant == null)
                return NotFound(new { success = false, message = "Müşteri bulunamadı." });

            var previousLogoUrl = tenant.LogoUrl;

            try
            {
                var webRootPath = _environment.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRootPath))
                {
                    webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
                }

                var uploadFolder = Path.Combine(webRootPath, "uploads");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var fullPath = Path.Combine(uploadFolder, fileName);

                await using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = "/uploads/" + fileName;

                var updateResult = await _tenantService.UpdateTenantAsync(tenantId.Value, new UpdateTenantDto
                {
                    LogoUrl = relativePath
                });

                if (!updateResult.IsSuccess)
                {
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);

                    return BadRequest(new { success = false, message = updateResult.ErrorMessage });
                }

                if (!string.IsNullOrWhiteSpace(previousLogoUrl)
                    && previousLogoUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(previousLogoUrl, relativePath, StringComparison.OrdinalIgnoreCase))
                {
                    var oldFileName = Path.GetFileName(previousLogoUrl);
                    var oldFilePath = Path.Combine(uploadFolder, oldFileName);
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                return Ok(new
                {
                    success = true,
                    message = "Logo başarıyla yüklendi.",
                    path = relativePath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Logo yüklenirken bir hata oluştu: " + ex.Message });
            }
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