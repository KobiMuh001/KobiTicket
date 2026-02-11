using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Application.Services;
using KobiMuhendislikTicket.Application.Common;
using KobiMuhendislikTicket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KobiMuhendislikTicket.Controllers
{
    [Authorize(Roles = "Staff")]
    [ApiController]
    [Route("api/staff")]
    public class StaffController : ControllerBase
    {
        private readonly StaffService _staffService;
        private readonly TicketService _ticketService;
        private readonly NotificationService _notificationService;
        private readonly ApplicationDbContext _context;

        public StaffController(
            StaffService staffService, 
            TicketService ticketService, 
            NotificationService notificationService,
            ApplicationDbContext context)
        {
            _staffService = staffService;
            _ticketService = ticketService;
            _notificationService = notificationService;
            _context = context;
        }

        private int GetStaffId()
        {
            var staffIdClaim = User.FindFirst("StaffId")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(staffIdClaim!);
        }

        // Staff profil bilgisi
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var staffId = GetStaffId();
            var profile = await _staffService.GetStaffProfileAsync(staffId);
            
            if (profile == null)
                return NotFound(new { success = false, message = "Profil bulunamadı." });

            return Ok(new { success = true, data = profile });
        }

        // Staff workload özeti
        [HttpGet("workload")]
        public async Task<IActionResult> GetMyWorkload()
        {
            var staffId = GetStaffId();
            var workload = await _staffService.GetMyWorkloadAsync(staffId);
            
            if (workload == null)
                return NotFound(new { success = false, message = "Workload bilgisi bulunamadı." });

            return Ok(new { success = true, data = workload });
        }

        // Staff'a atanmış ticketlar
        [HttpGet("tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var staffId = GetStaffId();
            var tickets = await _staffService.GetMyTicketsAsync(staffId);
            return Ok(new { success = true, data = tickets });
        }

        // Atanmamış (boş) ticketlar
        [HttpGet("tickets/unassigned")]
        public async Task<IActionResult> GetUnassignedTickets()
        {
            var tickets = await _staffService.GetUnassignedTicketsAsync();
            return Ok(new { success = true, data = tickets });
        }

        // Ticket'ı kendine al
        [HttpPost("tickets/{ticketId}/claim")]
        public async Task<IActionResult> ClaimTicket(int ticketId)
        {
            var staffId = GetStaffId();
            var result = await _staffService.ClaimTicketAsync(ticketId, staffId);

            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "Ticket başarıyla alındı." });
        }

        // Ticket'ı bırak
        [HttpPost("tickets/{ticketId}/release")]
        public async Task<IActionResult> ReleaseTicket(int ticketId)
        {
            var staffId = GetStaffId();
            var result = await _staffService.ReleaseTicketAsync(ticketId, staffId);

            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "Ticket bırakıldı." });
        }

        // Ticket detayı
        [HttpGet("tickets/{ticketId}")]
        public async Task<IActionResult> GetTicketDetail(int ticketId)
        {
            var staffId = GetStaffId();
            var result = await _ticketService.GetTicketByIdAsync(ticketId);
            
            if (!result.IsSuccess || result.Data == null)
                return NotFound(new { success = false, message = "Ticket bulunamadı." });

            var ticket = result.Data;
            var ticketDetail = new
            {
                id = ticket.Id.ToString(),
                ticketCode = ticket.TicketCode,
                title = ticket.Title,
                description = ticket.Description,
                status = (int)ticket.Status,
                priority = (int)ticket.Priority,
                creatorName = ticket.Tenant?.CompanyName ?? "Bilinmiyor",
                createdDate = ticket.CreatedDate,
                resolvedDate = ticket.UpdatedDate,
                imagePath = ticket.ImagePath,
                assignedPerson = ticket.AssignedPerson,
                asset = ticket.Asset != null ? new
                {
                    productName = ticket.Asset.ProductName,
                    serialNumber = ticket.Asset.SerialNumber,
                    status = ticket.Asset.Status
                } : null,
                solutionNote = (string?)null // TODO: Add solution note to Ticket entity if needed
            };

            return Ok(new { success = true, data = ticketDetail });
        }

        // Ticket'a yorum ekle
        [HttpPost("tickets/{ticketId}/comments")]
        public async Task<IActionResult> AddComment(int ticketId, [FromBody] CustomerCommentDto dto)
        {
            var staffId = GetStaffId();
            var profile = await _staffService.GetStaffProfileAsync(staffId);
            
            if (profile == null)
                return NotFound(new { success = false, message = "Profil bulunamadı." });

            var result = await _ticketService.AddCommentAsync(ticketId, dto.Message, profile.FullName, true, "Staff");
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            // Ticket başlığını almak için
            var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
            if (ticket.IsSuccess && ticket.Data != null)
            {
                await _notificationService.NotifyNewCommentAsync(ticketId, ticket.Data.Title, profile.FullName, true);
            }

            return Ok(new { success = true, message = "Yorum eklendi." });
        }

        // Ticket durumunu güncelle
        [HttpPut("tickets/{ticketId}/status")]
        public async Task<IActionResult> UpdateTicketStatus(int ticketId, [FromBody] UpdateTicketStatusDto dto)
        {
            var staffId = GetStaffId();
            var profile = await _staffService.GetStaffProfileAsync(staffId);
            
            if (profile == null)
                return NotFound(new { success = false, message = "Profil bulunamadı." });

            // Sadece kendi ticketlarının durumunu değiştirebilir
            var myTickets = await _staffService.GetMyTicketsAsync(staffId);
            if (!myTickets.Any(t => t.Id == ticketId))
                return Forbid();

            var result = await _ticketService.UpdateStatusAsync(ticketId, dto, profile.FullName);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "Ticket durumu güncellendi." });
        }

        // Ticket'ı çöz
        [HttpPost("tickets/{ticketId}/resolve")]
        public async Task<IActionResult> ResolveTicket(int ticketId, [FromBody] ResolveTicketDto dto)
        {
            var staffId = GetStaffId();
            var profile = await _staffService.GetStaffProfileAsync(staffId);
            
            if (profile == null)
                return NotFound(new { success = false, message = "Profil bulunamadı." });

            // Sadece kendi ticketlarını çözebilir
            var myTickets = await _staffService.GetMyTicketsAsync(staffId);
            if (!myTickets.Any(t => t.Id == ticketId))
                return Forbid();

            dto.ResolvedBy = profile.FullName;

            var result = await _ticketService.ResolveTicketAsync(ticketId, dto);
            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = "Ticket çözüldü." });
        }

        // Ticket geçmişi
        [HttpGet("tickets/{ticketId}/history")]
        public async Task<IActionResult> GetTicketHistory(int ticketId)
        {
            var history = await _ticketService.GetTicketHistoryAsync(ticketId);
            
            // Geçmiş zaten yerel saatte kaydedilmiş, dönüşüme gerek yok
            return Ok(new { success = true, data = history });
        }

        // Ticket yorumları
        [HttpGet("tickets/{ticketId}/comments")]
        public async Task<IActionResult> GetTicketComments(int ticketId)
        {
            var comments = await _ticketService.GetCommentsAsync(ticketId);
            
            // Yorumlar zaten yerel saatte kaydedilmiş, dönüşüme gerek yok
            return Ok(new { success = true, data = comments });
        }

        // ==================== BİLDİRİMLER ====================

        // Staff bildirimlerini getir
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] int take = 20)
        {
            var staffId = GetStaffId();
            var notifications = await _notificationService.GetStaffNotificationsAsync(staffId, take);
            return Ok(new { success = true, data = notifications, unreadCount = notifications.Count(n => !n.IsRead) });
        }

        // Bildirimi okundu olarak işaretle
        [HttpPatch("notifications/{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var staffId = GetStaffId();
            
            // Bildirimin bu staff'a ait olduğunu kontrol et
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound(new { success = false, message = "Bildirim bulunamadı." });
            
            if (notification.TargetUserId != staffId)
                return Forbid();
            
            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { success = true, message = "Bildirim okundu olarak işaretlendi." });
        }

        // Bildirimi sil
        [HttpDelete("notifications/{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var staffId = GetStaffId();
            
            // Bildirimin bu staff'a ait olduğunu kontrol et
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound(new { success = false, message = "Bildirim bulunamadı." });
            
            if (notification.TargetUserId != staffId)
                return Forbid();
            
            await _notificationService.DeleteNotificationAsync(id);
            return Ok(new { success = true, message = "Bildirim silindi." });
        }
    }
}
