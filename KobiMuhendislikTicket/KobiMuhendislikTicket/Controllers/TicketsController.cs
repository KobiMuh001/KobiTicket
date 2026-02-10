using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KobiMuhendislikTicket.Application.Services;
using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using KobiMuhendislikTicket.Infrastructure.Persistence;

namespace KobiMuhendislikTicket.Controllers
{
    [Authorize] // Sadece login olanlar bilet açabilir
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly TicketService _ticketService;
        private readonly NotificationService _notificationService;
        private readonly ApplicationDbContext _context;

        public TicketsController(
            TicketService ticketService, 
            NotificationService notificationService,
            ApplicationDbContext context)
        {
            _ticketService = ticketService;
            _notificationService = notificationService;
            _context = context;
        }

        [HttpPost("create-ticket")]
        public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
        {
            var tenantIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (tenantIdClaim == null) 
                return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });

            if (!Guid.TryParse(tenantIdClaim.Value, out var tenantId))
                return Unauthorized(new { message = "Geçersiz kullanıcı kimliği." });

            // Rate limiting: Son 1 saatte maksimum 5 ticket kontrol
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            var ticketCountInLastHour = await _context.Tickets
                .Where(t => t.TenantId == tenantId && t.CreatedDate >= oneHourAgo && !t.IsDeleted)
                .CountAsync();

            if (ticketCountInLastHour >= 5)
                return StatusCode(429, new { message = "Bir saatte maksimum 5 destek talebı açabilirsiniz. Lütfen daha sonra tekrar deneyiniz." });

            var ticket = new Ticket
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = (Domain.Enums.TicketPriority)dto.Priority,
                AssetId = dto.AssetId, 
                TenantId = tenantId    
            };

            var result = await _ticketService.CreateTicketAsync(ticket);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            // Bildirim oluştur
            var tenant = await _context.Tenants.FindAsync(tenantId);
            var tenantName = tenant?.CompanyName ?? "Müşteri";
            await _notificationService.NotifyNewTicketAsync(ticket, tenantName);

            return Ok(new { message = "Destek talebiniz başarıyla alındı.", ticketId = ticket.Id });
        }

        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) 
                return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });
            
            if (!Guid.TryParse(userIdClaim.Value, out var tenantId))
                return Unauthorized(new { message = "Geçersiz kullanıcı kimliği." });

            var tickets = await _ticketService.GetTenantTicketsAsync(tenantId);
            return Ok(tickets);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all-tickets")]
        public async Task<IActionResult> GetAllTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _ticketService.GetAllTicketsPagedAsync(page, pageSize);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(result.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("admin/update-status/{id}")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] int newStatus)
        {
            var result = await _ticketService.UpdateTicketStatusAsync(id, newStatus);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "Bilet durumu başarıyla güncellendi." });
        }

        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Dosya seçilmedi." });

            // Dosya türü kontrol
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new { message = "Yalnızca resim dosyaları (jpg, png, gif, webp) yüklenebilir." });

            // Dosya boyutu kontrol (5MB max)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Dosya boyutu 5MB'ı geçemez." });

            try
            {
                // 1. Klasör yolunu ayarla (wwwroot/uploads)
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                // 2. Benzersiz dosya ismi oluştur
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var fullPath = Path.Combine(uploadFolder, fileName);

                // 3. Dosyayı fiziksel olarak kaydet
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 4. Veritabanı yolunu oluştur (Tarayıcıdan erişilecek yol)
                string relativePath = "/uploads/" + fileName;

                var result = await _ticketService.SaveTicketImageAsync(id, relativePath);
                
                if (!result.IsSuccess)
                {
                    // Dosya kaydedilemedi, fiziksel dosyayı sil
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                    
                    return BadRequest(new { message = result.ErrorMessage });
                }

                return Ok(new
                {
                    message = "Dosya başarıyla yüklendi.",
                    path = relativePath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Dosya yüklenirken bir hata oluştu: " + ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("admin/assign/{id}")]
        public async Task<IActionResult> AssignTicket(Guid id, [FromBody] string personName)
        {
            var result = await _ticketService.AssignTicketToPersonAsync(id, personName);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = $"{personName} personeline atandı ve durum 'İşlemde' olarak güncellendi." });
        }

        // Müşteri için yorum listesi
        [HttpGet("{ticketId}/comments")]
        public async Task<IActionResult> GetComments(Guid ticketId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) 
                return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });
            
            if (!Guid.TryParse(userIdClaim.Value, out var tenantId))
                return Unauthorized(new { message = "Geçersiz kullanıcı kimliği." });

            // Ticket'ın bu müşteriye ait olduğunu kontrol et
            var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
            if (!ticket.IsSuccess || ticket.Data?.TenantId != tenantId)
                return NotFound(new { success = false, message = "Ticket bulunamadı." });

            var comments = await _ticketService.GetCommentsAsync(ticketId);
            return Ok(new { success = true, data = comments });
        }

        // Müşteri için yorum ekleme
        [HttpPost("{ticketId}/comments")]
        public async Task<IActionResult> AddComment(Guid ticketId, [FromBody] CustomerCommentDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) 
                return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });
            
            if (!Guid.TryParse(userIdClaim.Value, out var tenantId))
                return Unauthorized(new { message = "Geçersiz kullanıcı kimliği." });

            // Ticket'ın bu müşteriye ait olduğunu kontrol et
            var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
            if (!ticket.IsSuccess || ticket.Data?.TenantId != tenantId)
                return NotFound(new { success = false, message = "Ticket bulunamadı." });

            // Müşteri adını al
            var tenant = await _context.Tenants.FindAsync(tenantId);
            var authorName = tenant?.CompanyName ?? "Müşteri";

            var result = await _ticketService.AddCommentAsync(ticketId, dto.Message, authorName, false, "Customer");
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            // Admin'e bildirim gönder
            await _notificationService.NotifyNewCommentAsync(ticketId, ticket.Data.Title, authorName, true);

            return Ok(new { success = true, message = "Yorum eklendi." });
        }
    }
}